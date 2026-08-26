using Cleanifico.Domain.CleaningObjects;

namespace Cleanifico.Application.CleaningObjects;

public sealed record CleaningObjectRecord(
    CleaningObject CleaningObject,
    string CustomerNumber,
    string CustomerCompanyName);
