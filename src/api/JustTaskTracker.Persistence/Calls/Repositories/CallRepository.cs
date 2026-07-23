using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Domain.Calls.Entities;
using JustTaskTracker.Persistence.Common;

namespace JustTaskTracker.Persistence.Calls.Repositories;

public class CallRepository(JustTaskTrackerDbContext context) : Repository<CallSession, Guid>(context), ICallRepository;
