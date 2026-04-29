using MediatR;
using Microsoft.EntityFrameworkCore;
using WEBDULICH.Models;
using WEBDULICH.Services;

namespace WEBDULICH.Queries
{
    public class GetTourByIdQuery : IRequest<Tour?>
    {
        public int TourId { get; set; }

        public GetTourByIdQuery(int tourId)
        {
            TourId = tourId;
        }
    }

    public class GetTourByIdQueryHandler : IRequestHandler<GetTourByIdQuery, Tour?>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetTourByIdQueryHandler> _logger;

        public GetTourByIdQueryHandler(ApplicationDbContext context, ILogger<GetTourByIdQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Tour?> Handle(GetTourByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Fetching tour with ID {request.TourId}");

            var tour = await _context.Tours
                .Include(t => t.Destination)
                .Include(t => t.Reviews)
                .FirstOrDefaultAsync(t => t.Id == request.TourId, cancellationToken);

            if (tour == null)
            {
                _logger.LogWarning($"Tour with ID {request.TourId} not found");
            }

            return tour;
        }
    }

    public class GetAllToursQuery : IRequest<List<Tour>>
    {
        public bool IncludeInactive { get; set; } = false;
    }

    public class GetAllToursQueryHandler : IRequestHandler<GetAllToursQuery, List<Tour>>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetAllToursQueryHandler> _logger;

        public GetAllToursQueryHandler(ApplicationDbContext context, ILogger<GetAllToursQueryHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Tour>> Handle(GetAllToursQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all tours");

            var query = _context.Tours
                .Include(t => t.Destination)
                .AsQueryable();

            // Filter by IsActive if the property exists
            // if (!request.IncludeInactive)
            // {
            //     query = query.Where(t => t.IsActive);
            // }

            var tours = await query.ToListAsync(cancellationToken);

            _logger.LogInformation($"Found {tours.Count} tours");

            return tours;
        }
    }
}
