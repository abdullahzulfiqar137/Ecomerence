USE [ECommerceDb];
GO
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- 1. Disable all foreign key constraints
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- 2. Clear Audit, Notifications & Inventory
DELETE FROM [AuditLogs];
DELETE FROM [Notifications];
DELETE FROM [LowStockAlerts];
DELETE FROM [StockMovements];
DELETE FROM [StockAdjustments];
DELETE FROM [InventoryReservations];
DELETE FROM [InventoryItems];
DELETE FROM [WarehouseLocations];
DELETE FROM [Warehouses];

-- 3. Clear Payments, Tracking & Shipments
DELETE FROM [TrackingEvents];
DELETE FROM [ShipmentItems];
DELETE FROM [Shipments];
DELETE FROM [ShippingRates];
DELETE FROM [ShippingMethods];
DELETE FROM [ShippingZones];
DELETE FROM [Payments];

-- 4. Clear Orders & Order Sub-Entities
DELETE FROM [OrderStatusHistories];
DELETE FROM [OrderNotes];
DELETE FROM [OrderItems];
DELETE FROM [Orders];
DELETE FROM [OrderAddresses];

-- 5. Clear Carts & Wishlists
DELETE FROM [CartItems];
DELETE FROM [Carts];
DELETE FROM [WishlistItems];
DELETE FROM [Wishlists];

-- 6. Clear Reviews, Questions & Answers
DELETE FROM [ReviewUpvotes];
DELETE FROM [ProductReviews];
DELETE FROM [ProductAnswers];
DELETE FROM [ProductQuestions];

-- 7. Clear Products, Variants, Images & Attributes
DELETE FROM [ProductBundleItems];
DELETE FROM [ProductRelatedProducts];
DELETE FROM [ProductSpecifications];
DELETE FROM [ProductVideos];
DELETE FROM [ProductTags];
DELETE FROM [ProductImages];
DELETE FROM [ProductVariantImages];
DELETE FROM [ProductAttributeValues];
DELETE FROM [ProductAttributes];
DELETE FROM [ProductVariants];
DELETE FROM [Products];
DELETE FROM [Categories];
DELETE FROM [Brands];

-- 8. Clear Promotions, Coupons & Tax Rules
DELETE FROM [PromotionCategories];
DELETE FROM [PromotionProducts];
DELETE FROM [Promotions];
DELETE FROM [CouponUsages];
DELETE FROM [Coupons];
DELETE FROM [DiscountRules];
DELETE FROM [TaxRules];

-- 9. Clean non-admin test users, addresses, refresh tokens
DELETE FROM [RefreshTokens];
DELETE FROM [UserRoles] WHERE UserId NOT IN (SELECT Id FROM [Users] WHERE Email = 'admin@ecommerce.global');
DELETE FROM [CustomerProfiles] WHERE UserId NOT IN (SELECT Id FROM [Users] WHERE Email = 'admin@ecommerce.global');
DELETE FROM [Addresses] WHERE UserId NOT IN (SELECT Id FROM [Users] WHERE Email = 'admin@ecommerce.global');
DELETE FROM [Users] WHERE Email <> 'admin@ecommerce.global';
GO

-- 10. Re-enable all foreign key constraints
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";
GO

PRINT 'DATABASE CLEANED: All business data deleted. Admin user and schema retained.';
GO
