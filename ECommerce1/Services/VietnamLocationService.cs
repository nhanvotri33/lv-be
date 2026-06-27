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
        private static readonly HttpClient _httpClient = new HttpClient();
        private static VietnamProvinceApiResponse? _cachedData;

        private static async Task LoadDataAsync()
        {
            if (_cachedData != null) return;

            try
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

        public static async Task MigrateAllLocationsAsync(ApplicationDbContext context)
        {
            await LoadDataAsync();
            if (_cachedData?.Data == null) return;

            // 1. Get existing provinces and wards in DB
            var existingProvinces = await context.Provinces.ToDictionaryAsync(p => p.Id);
            var existingWards = await context.Wards.ToDictionaryAsync(w => w.Id);

            var newProvinces = new List<Province>();
            var newWards = new List<Ward>();

            // 2. Loop through VietnamLabs data
            foreach (var provinceData in _cachedData.Data)
            {
                if (string.IsNullOrEmpty(provinceData.Id)) continue;

                if (!existingProvinces.ContainsKey(provinceData.Id))
                {
                    var province = new Province
                    {
                        Id = provinceData.Id,
                        Name = provinceData.Province ?? string.Empty,
                        FullName = provinceData.Province ?? string.Empty,
                        CodeName = provinceData.Province ?? string.Empty
                    };
                    newProvinces.Add(province);
                    existingProvinces.Add(province.Id, province);
                }

                if (provinceData.Wards != null)
                {
                    foreach (var wardData in provinceData.Wards)
                    {
                        if (string.IsNullOrEmpty(wardData.Name)) continue;
                        var wardId = GetStableId($"{provinceData.Id}_{wardData.Name}");

                        if (!existingWards.ContainsKey(wardId))
                        {
                            var ward = new Ward
                            {
                                Id = wardId,
                                Name = wardData.Name,
                                FullName = wardData.Name,
                                CodeName = wardData.Name,
                                ProvinceId = provinceData.Id
                            };
                            newWards.Add(ward);
                            existingWards.Add(ward.Id, ward);
                        }
                    }
                }
            }

            // 3. Save new provinces in batches
            if (newProvinces.Any())
            {
                context.Provinces.AddRange(newProvinces);
                await context.SaveChangesAsync();
            }

            // 4. Save new wards in batches of 500 to avoid parameter limit issues
            if (newWards.Any())
            {
                const int batchSize = 500;
                for (int i = 0; i < newWards.Count; i += batchSize)
                {
                    var batch = newWards.Skip(i).Take(batchSize).ToList();
                    context.Wards.AddRange(batch);
                    await context.SaveChangesAsync();
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
