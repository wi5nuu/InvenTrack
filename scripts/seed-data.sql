-- ============================================
-- InvenTrack Enterprise - Seed Data Script
-- Run after tables are created via EF Core
-- ============================================

-- Users (passwords hashed with BCrypt)
INSERT INTO Users (Username, Email, PasswordHash, PasswordSalt, Role, IsActive, CreatedAt)
VALUES
('admin', 'admin@inventrack.local', '$2a$12$LJ3m4ys3Lk0TSwMCfVCXae5vJ8fMqbDhMGKGeOZ5sQqO4CRp0KxSu', '$2a$12$LJ3m4ys3Lk0TSwMCfVCXae', 'Admin', 1, GETUTCDATE()),
('manager', 'manager@inventrack.local', '$2a$12$q7p4GHgQlX7sJ8fMqbDhMGKGeOZ5sQqO4CRp0KxSuLJ3m4ys3Lk0T', '$2a$12$q7p4GHgQlX7sJ8fMqbDhMG', 'Manager', 1, GETUTCDATE()),
('staff', 'staff@inventrack.local', '$2a$12$s5v8J8fMqbDhMGKGeOZ5sQqO4CRp0KxSuLJ3m4ys3Lk0TSwMCfVCX', '$2a$12$s5v8J8fMqbDhMGKGeOZ5s', 'Staff', 1, GETUTCDATE());

-- Categories
INSERT INTO Categories (Name, Description, IsActive, CreatedAt)
VALUES
('Electronics', 'Electronic devices and components', 1, GETUTCDATE()),
('Office Supplies', 'Office and stationery items', 1, GETUTCDATE()),
('Raw Materials', 'Raw materials for manufacturing', 1, GETUTCDATE()),
('Finished Goods', 'Ready-to-ship finished products', 1, GETUTCDATE()),
('Spare Parts', 'Replacement and spare parts', 1, GETUTCDATE());

-- Suppliers
INSERT INTO Suppliers (Name, ContactPerson, Email, Phone, Address, IsActive, CreatedAt)
VALUES
('TechSupply Co', 'John Smith', 'john@techsupply.com', '+1-555-0101', '123 Tech Street, Silicon Valley, CA', 1, GETUTCDATE()),
('OfficeMart Inc', 'Jane Doe', 'jane@officemart.com', '+1-555-0102', '456 Business Ave, New York, NY', 1, GETUTCDATE()),
('GlobalParts Ltd', 'Bob Wilson', 'bob@globalparts.com', '+1-555-0103', '789 Industrial Blvd, Chicago, IL', 1, GETUTCDATE());
