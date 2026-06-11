namespace InvenTrack.Domain.Enums;

/// <summary>Stock movement direction — IN (restock), OUT (sale/use), ADJUSTMENT (correction), IMPORT (bulk).</summary>
public enum MovementType
{
    IN,
    OUT,
    ADJUSTMENT,
    IMPORT
}
