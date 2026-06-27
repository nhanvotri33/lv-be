using ECommerce.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ECommerce1.Services
{
    public class ProvinceDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? FullName { get; set; }
        public string? CodeName { get; set; }
    }

    public class WardDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? FullName { get; set; }
        public string? CodeName { get; set; }
        public string? ProvinceId { get; set; }
    }

    public static class VietnamLocationService
    {
        //khai báo biến http 
        private static readonly HttpClient _httpClient = new HttpClient();
        // khai báo biến cache để lưu data load lên được -> load vô RAM
        private static VietnamProvinceApiResponse? _cachedData;

        // hàm này dùng để load data từ API vào RAM
        private static async Task LoadDataAsync()
        {
            // nếu đã load data rồi thì không load nữa
            if (_cachedData != null) return;

            try // xử lý lỗi khi load data
            {
                var response = await _httpClient.GetFromJsonAsync<VietnamProvinceApiResponse>("https://vietnamlabs.com/api/vietnamprovince");
                if (response != null && response.Success)
                {
                    _cachedData = response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching vietnam province API: " + ex.Message);
            }
        }

        public static string GetStableId(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                var value = BitConverter.ToUInt32(bytes, 0);
                return value.ToString();
            }
        }

        public static async Task<List<ProvinceDto>> GetProvincesAsync()
        {
            await LoadDataAsync();
            if (_cachedData?.Data == null) return new List<ProvinceDto>();

            return _cachedData.Data.Select(p => new ProvinceDto
            {
                Id = p.Id,
                Name = p.Province,
                FullName = p.Province,
                CodeName = p.Province
            }).OrderBy(p => p.Name).ToList();
        }

        public static async Task<List<WardDto>> GetWardsByProvinceAsync(string provinceId)
        {
            await LoadDataAsync();
            if (_cachedData?.Data == null) return new List<WardDto>();

            var province = _cachedData.Data.FirstOrDefault(p => p.Id == provinceId);
            if (province == null || province.Wards == null) return new List<WardDto>();

            return province.Wards.Select(w => {
                var wardId = GetStableId($"{provinceId}_{w.Name}");
                return new WardDto
                {
                    Id = wardId,
                    Name = w.Name,
                    FullName = w.Name,
                    CodeName = w.Name,
                    ProvinceId = provinceId
                };
            }).OrderBy(w => w.Name).ToList();
        }

        public static async Task EnsureLocationExistsAsync(ApplicationDbContext context, string? wardId)
        {
            if (string.IsNullOrEmpty(wardId)) return;

            // 1. Check if the ward already exists in the database
            var wardExists = await context.Wards.AnyAsync(w => w.Id == wardId);
            if (wardExists) return;

            // 2. Load API data to find the ward
            await LoadDataAsync();
            if (_cachedData?.Data == null) return;

            // 3. Find the ward in our cached data
            foreach (var province in _cachedData.Data)
            {
                if (province.Wards == null || string.IsNullOrEmpty(province.Id)) continue;
                foreach (var ward in province.Wards)
                {
                    if (string.IsNullOrEmpty(ward.Name)) continue;
                    var calculatedWardId = GetStableId($"{province.Id}_{ward.Name}");
                    if (calculatedWardId == wardId)
                    {
                        // Found! Ensure the Province exists first
                        var dbProvince = await context.Provinces.FindAsync(province.Id);
                        if (dbProvince == null)
                        {
                            dbProvince = new Province
                            {
                                Id = province.Id,
                                Name = province.Province ?? string.Empty,
                                FullName = province.Province ?? string.Empty,
                                CodeName = province.Province ?? string.Empty
                            };
                            context.Provinces.Add(dbProvince);
                            await context.SaveChangesAsync(); // save province first
                        }

                        // Now ensure the Ward exists
                        var dbWard = await context.Wards.FindAsync(wardId);
                        if (dbWard == null)
                        {
                            dbWard = new Ward
                            {
                                Id = wardId,
                                Name = ward.Name ?? string.Empty,
                                FullName = ward.Name ?? string.Empty,
                                CodeName = ward.Name ?? string.Empty,
                                ProvinceId = province.Id
                            };
                            context.Wards.Add(dbWard);
                            await context.SaveChangesAsync(); // save ward
                        }
                        return;
                    }
                }
            }
        }
    }

    public class VietnamProvinceApiResponse
    {
        public bool Success { get; set; }
        public List<VietnamProvinceData>? Data { get; set; }
    }

    public class VietnamProvinceData
    {
        public string? Province { get; set; }
        public string? Id { get; set; }
        public List<VietnamWardData>? Wards { get; set; }
    }

    public class VietnamWardData
    {
        public string? Name { get; set; }
    }
}
