using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Calls.Entities;

namespace JustTaskTracker.Application.Calls.Repositories;

public interface ICallRepository : IRepository<CallSession, Guid>;
