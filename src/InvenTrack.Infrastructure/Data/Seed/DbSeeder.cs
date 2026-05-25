using InvenTrack.Domain.Entities;
using InvenTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InvenTrack.Infrastructure.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        // Seed users
        var users = new List<User>
        {
            new()
            {
                Username = "admin",
                Email = "admin@inventrack.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456", 12),
                PasswordSalt = BCrypt.Net.BCrypt.GenerateSalt(12),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Username = "manager",
                Email = "manager@inventrack.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123456", 12),
                PasswordSalt = BCrypt.Net.BCrypt.GenerateSalt(12),
                Role = UserRole.Manager,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Username = "staff",
                Email = "staff@inventrack.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123456", 12),
                PasswordSalt = BCrypt.Net.BCrypt.GenerateSalt(12),
                Role = UserRole.Staff,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        // Seed categories
        var categories = new List<Category>
        {
            new() { Name = "Electronics", Description = "Electronic devices and components", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Office Supplies", Description = "Office and stationery items", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Raw Materials", Description = "Raw materials for manufacturing", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Finished Goods", Description = "Ready-to-ship finished products", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Name = "Spare Parts", Description = "Replacement and spare parts", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // Seed suppliers
        var suppliers = new List<Supplier>
        {
            new()
            {
                Name = "TechSupply Co",
                ContactPerson = "John Smith",
                Email = "john@techsupply.com",
                Phone = "+1-555-0101",
                Address = "123 Tech Street, Silicon Valley, CA",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "OfficeMart Inc",
                ContactPerson = "Jane Doe",
                Email = "jane@officemart.com",
                Phone = "+1-555-0102",
                Address = "456 Business Ave, New York, NY",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "GlobalParts Ltd",
                ContactPerson = "Bob Wilson",
                Email = "bob@globalparts.com",
                Phone = "+1-555-0103",
                Address = "789 Industrial Blvd, Chicago, IL",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        await context.Suppliers.AddRangeAsync(suppliers);
        await context.SaveChangesAsync();

        // Seed products
        var adminUser = users[0];
        var products = new List<Product>
        {
            new() { SKU = "ELEC-LAP-001", Name = "Pro Notebook 15\"", Description = "High-performance laptop with 16GB RAM", CategoryId = categories[0].Id, SupplierId = suppliers[0].Id, UnitPrice = 1299.99m, CostPrice = 950.00m, StockQuantity = 45, MinStockThreshold = 10, Unit = "pcs", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "ELEC-LAP-002", Name = "UltraBook 14\"", Description = "Lightweight ultrabook for professionals", CategoryId = categories[0].Id, SupplierId = suppliers[0].Id, UnitPrice = 999.99m, CostPrice = 750.00m, StockQuantity = 30, MinStockThreshold = 8, Unit = "pcs", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "ELEC-MON-001", Name = "27\" 4K Monitor", Description = "Ultra HD monitor with USB-C", CategoryId = categories[0].Id, SupplierId = suppliers[0].Id, UnitPrice = 449.99m, CostPrice = 320.00m, StockQuantity = 3, MinStockThreshold = 5, Unit = "pcs", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "ELEC-KEY-001", Name = "Mechanical Keyboard", Description = "RGB mechanical keyboard", CategoryId = categories[0].Id, SupplierId = suppliers[0].Id, UnitPrice = 89.99m, CostPrice = 55.00m, StockQuantity = 0, MinStockThreshold = 20, Unit = "pcs", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "OFFC-PPR-001", Name = "A4 Printer Paper", Description = "500-sheet ream of A4 paper", CategoryId = categories[1].Id, SupplierId = suppliers[1].Id, UnitPrice = 5.99m, CostPrice = 3.50m, StockQuantity = 200, MinStockThreshold = 50, Unit = "pcs", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "OFFC-PEN-001", Name = "Ballpoint Pen Box", Description = "Box of 12 blue ballpoint pens", CategoryId = categories[1].Id, SupplierId = suppliers[1].Id, UnitPrice = 4.99m, CostPrice = 2.20m, StockQuantity = 150, MinStockThreshold = 30, Unit = "box", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "OFFC-DSK-001", Name = "Standing Desk", Description = "Electric height-adjustable desk", CategoryId = categories[1].Id, SupplierId = suppliers[1].Id, UnitPrice = 599.99m, CostPrice = 400.00m, StockQuantity = 12, MinStockThreshold = 5, Unit = "pcs", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "OFFC-CHA-001", Name = "Ergonomic Chair", Description = "Mesh back ergonomic office chair", CategoryId = categories[1].Id, SupplierId = suppliers[1].Id, UnitPrice = 349.99m, CostPrice = 220.00m, StockQuantity = 8, MinStockThreshold = 5, Unit = "pcs", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "RAW-STL-001", Name = "Steel Sheet 4x8", Description = "Galvanized steel sheet 4ft x 8ft", CategoryId = categories[2].Id, SupplierId = suppliers[2].Id, UnitPrice = 45.00m, CostPrice = 28.00m, StockQuantity = 500, MinStockThreshold = 100, Unit = "pcs", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "RAW-ALU-001", Name = "Aluminum Rod 1m", Description = "6061 aluminum rod, 1 meter length", CategoryId = categories[2].Id, SupplierId = suppliers[2].Id, UnitPrice = 12.50m, CostPrice = 7.50m, StockQuantity = 1000, MinStockThreshold = 200, Unit = "pcs", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "RAW-PLA-001", Name = "Polypropylene Pellets", Description = "Industrial grade PP pellets (25kg bag)", CategoryId = categories[2].Id, SupplierId = suppliers[2].Id, UnitPrice = 35.00m, CostPrice = 22.00m, StockQuantity = 80, MinStockThreshold = 30, Unit = "bag", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "RAW-WIR-001", Name = "Copper Wire Spool", Description = "AWG 14 copper wire, 100ft spool", CategoryId = categories[2].Id, SupplierId = suppliers[2].Id, UnitPrice = 28.00m, CostPrice = 18.00m, StockQuantity = 1, MinStockThreshold = 15, Unit = "spool", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "FNSH-GDT-001", Name = "Widget A-200", Description = "Standard assembly widget model A-200", CategoryId = categories[3].Id, SupplierId = suppliers[0].Id, UnitPrice = 24.99m, CostPrice = 14.50m, StockQuantity = 350, MinStockThreshold = 50, Unit = "pcs", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "FNSH-GDT-002", Name = "Gadget Pro X", Description = "Next-gen gadget with smart features", CategoryId = categories[3].Id, SupplierId = suppliers[0].Id, UnitPrice = 79.99m, CostPrice = 45.00m, StockQuantity = 120, MinStockThreshold = 25, Unit = "pcs", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "FNSH-GDT-003", Name = "EcoLight Bulb 10W", Description = "Energy-efficient LED bulb 10W equivalent", CategoryId = categories[3].Id, SupplierId = suppliers[1].Id, UnitPrice = 3.99m, CostPrice = 1.80m, StockQuantity = 0, MinStockThreshold = 100, Unit = "pcs", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "FNSH-GDT-004", Name = "Smart Thermostat", Description = "WiFi-enabled smart home thermostat", CategoryId = categories[3].Id, SupplierId = suppliers[0].Id, UnitPrice = 149.99m, CostPrice = 95.00m, StockQuantity = 4, MinStockThreshold = 10, Unit = "pcs", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "SPR-BRG-001", Name = "Ball Bearing 6205", Description = "Deep groove ball bearing 25x52x15mm", CategoryId = categories[4].Id, SupplierId = suppliers[2].Id, UnitPrice = 3.50m, CostPrice = 1.75m, StockQuantity = 500, MinStockThreshold = 100, Unit = "pcs", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "SPR-BLT-001", Name = "Hex Bolt M8x30", Description = "Grade 8.8 hex bolt, M8x30mm, pack of 100", CategoryId = categories[4].Id, SupplierId = suppliers[2].Id, UnitPrice = 8.99m, CostPrice = 4.50m, StockQuantity = 300, MinStockThreshold = 50, Unit = "pack", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "SPR-BLT-002", Name = "Hex Bolt M10x40", Description = "Grade 8.8 hex bolt, M10x40mm, pack of 50", CategoryId = categories[4].Id, SupplierId = suppliers[2].Id, UnitPrice = 7.49m, CostPrice = 3.80m, StockQuantity = 200, MinStockThreshold = 40, Unit = "pack", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SKU = "SPR-FLT-001", Name = "Flat Washer M8", Description = "Flat washer M8, pack of 200", CategoryId = categories[4].Id, SupplierId = suppliers[2].Id, UnitPrice = 4.99m, CostPrice = 2.50m, StockQuantity = 600, MinStockThreshold = 100, Unit = "pack", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        // Seed stock movements (30 records for chart data)
        var random = new Random(42);
        var movements = new List<StockMovement>();
        for (int i = 0; i < 30; i++)
        {
            var product = products[random.Next(products.Count)];
            var qtyBefore = product.StockQuantity + random.Next(1, 50);
            var qty = random.Next(1, 20);
            var isIn = random.Next(2) == 0;
            var movement = new StockMovement
            {
                ProductId = product.Id,
                MovementType = isIn ? MovementType.IN : MovementType.OUT,
                Quantity = qty,
                QuantityBefore = qtyBefore,
                QuantityAfter = isIn ? qtyBefore + qty : qtyBefore - qty,
                Reference = isIn ? "Purchase order" : "Sales order",
                Notes = isIn ? "Stock replenishment" : "Customer order fulfillment",
                UserId = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)).AddHours(random.Next(1, 24))
            };
            movements.Add(movement);
        }
        await context.StockMovements.AddRangeAsync(movements);
        await context.SaveChangesAsync();
    }
}
