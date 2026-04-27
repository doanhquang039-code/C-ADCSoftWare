using Microsoft.AspNetCore.Mvc;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Controllers
{
    public class MapController : Controller
    {
        private readonly IMapService _mapService;
        private readonly ITourService _tourService;
        private readonly IHotelService _hotelService;

        public MapController(IMapService mapService, ITourService tourService, IHotelService hotelService)
        {
            _mapService = mapService;
            _tourService = tourService;
            _hotelService = hotelService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Tours()
        {
            await _mapService.SyncTourLocationsAsync();
            var tourLocations = await _mapService.GetLocationsByTypeAsync("Tour");
            return View(tourLocations);
        }

        public async Task<IActionResult> Hotels()
        {
            await _mapService.SyncHotelLocationsAsync();
            var hotelLocations = await _mapService.GetLocationsByTypeAsync("Hotel");
            return View(hotelLocations);
        }

        public async Task<IActionResult> Attractions()
        {
            var attractions = await _mapService.GetLocationsByTypeAsync("Attraction");
            return View(attractions);
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] MapSearchRequest request)
        {
            try
            {
                var result = await _mapService.SearchLocationsAsync(request);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLocation(int id)
        {
            try
            {
                var location = await _mapService.GetLocationByIdAsync(id);
                if (location == null)
                {
                    return Json(new { success = false, message = "Location not found" });
                }

                return Json(new { success = true, data = location });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetRoute([FromBody] RouteRequest request)
        {
            try
            {
                var route = await _mapService.GetRouteAsync(request);
                return Json(new { success = true, data = route });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNearbyAttractions(double lat, double lon, double radius = 5)
        {
            try
            {
                var attractions = await _mapService.GetNearbyAttractionsAsync(lat, lon, radius);
                return Json(new { success = true, data = attractions });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Geocode([FromBody] string address)
        {
            try
            {
                var (lat, lon) = await _mapService.GeocodeAddressAsync(address);
                return Json(new { success = true, latitude = lat, longitude = lon });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReverseGeocode(double lat, double lon)
        {
            try
            {
                var address = await _mapService.ReverseGeocodeAsync(lat, lon);
                return Json(new { success = true, address = address });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Admin functions
        public async Task<IActionResult> Manage()
        {
            var locations = await _mapService.SearchLocationsAsync(new MapSearchRequest { PageSize = 100 });
            return View(locations.Locations);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Location location)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // If no coordinates provided, try to geocode the address
                    if (location.Latitude == 0 && location.Longitude == 0 && !string.IsNullOrEmpty(location.Address))
                    {
                        var (lat, lon) = await _mapService.GeocodeAddressAsync(location.Address);
                        location.Latitude = lat;
                        location.Longitude = lon;
                    }

                    await _mapService.CreateLocationAsync(location);
                    TempData["Success"] = "Location created successfully!";
                    return RedirectToAction("Manage");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating location: {ex.Message}";
                }
            }

            return View(location);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var location = await _mapService.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Location location)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _mapService.UpdateLocationAsync(location);
                    TempData["Success"] = "Location updated successfully!";
                    return RedirectToAction("Manage");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error updating location: {ex.Message}";
                }
            }

            return View(location);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _mapService.DeleteLocationAsync(id);
                TempData["Success"] = "Location deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting location: {ex.Message}";
            }

            return RedirectToAction("Manage");
        }

        [HttpPost]
        public async Task<IActionResult> SyncTours()
        {
            try
            {
                await _mapService.SyncTourLocationsAsync();
                TempData["Success"] = "Tour locations synced successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error syncing tours: {ex.Message}";
            }

            return RedirectToAction("Manage");
        }

        [HttpPost]
        public async Task<IActionResult> SyncHotels()
        {
            try
            {
                await _mapService.SyncHotelLocationsAsync();
                TempData["Success"] = "Hotel locations synced successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error syncing hotels: {ex.Message}";
            }

            return RedirectToAction("Manage");
        }
    }
}