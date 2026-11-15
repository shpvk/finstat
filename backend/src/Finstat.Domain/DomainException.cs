namespace Finstat.Domain;

/// <summary>Нарушение доменного инварианта; API превращает его в 400.</summary>
public class DomainException(string message) : Exception(message);
