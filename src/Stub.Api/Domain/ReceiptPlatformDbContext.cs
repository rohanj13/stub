using Microsoft.EntityFrameworkCore;

namespace Stub.Api.Domain;

public sealed class ReceiptPlatformDbContext(DbContextOptions<ReceiptPlatformDbContext> options) : DbContext(options)
{
    // Core entities
    public DbSet<MerchantPortalConnectionEntity> Merchants => Set<MerchantPortalConnectionEntity>();
    public DbSet<DigitalReceiptEntity> Receipts => Set<DigitalReceiptEntity>();
    public DbSet<ReceiptLineItemEntity> ReceiptItems => Set<ReceiptLineItemEntity>();
    
    // Merchant customization
    public DbSet<MerchantCustomizationEntity> MerchantCustomizations => Set<MerchantCustomizationEntity>();
    
    // Receipt envelope and modules
    public DbSet<ReceiptEnvelopeEntity> ReceiptEnvelopes => Set<ReceiptEnvelopeEntity>();
    public DbSet<ReceiptLoyaltyModuleEntity> ReceiptLoyaltyModules => Set<ReceiptLoyaltyModuleEntity>();
    public DbSet<ReceiptWarrantyModuleEntity> ReceiptWarrantyModules => Set<ReceiptWarrantyModuleEntity>();
    public DbSet<ReceiptAdModuleEntity> ReceiptAdModules => Set<ReceiptAdModuleEntity>();
    public DbSet<ReceiptIntegrationReferenceEntity> ReceiptIntegrationReferences => Set<ReceiptIntegrationReferenceEntity>();
    
    // Consent and contact management
    public DbSet<CustomerConsentEntity> CustomerConsents => Set<CustomerConsentEntity>();
    public DbSet<CustomerContactEntity> CustomerContacts => Set<CustomerContactEntity>();
    
    // Access control and security
    public DbSet<ReceiptClaimabilityEntity> ReceiptClaimabilities => Set<ReceiptClaimabilityEntity>();
    public DbSet<ReceiptAccessLogEntity> ReceiptAccessLogs => Set<ReceiptAccessLogEntity>();
    public DbSet<ReceiptRateLimitEntity> ReceiptRateLimits => Set<ReceiptRateLimitEntity>();
    
    // Compliance and verification
    public DbSet<ReceiptComplianceProfileEntity> ReceiptComplianceProfiles => Set<ReceiptComplianceProfileEntity>();
    public DbSet<ReceiptVerificationEntity> ReceiptVerifications => Set<ReceiptVerificationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Merchant
        modelBuilder.Entity<MerchantPortalConnectionEntity>(entity =>
        {
            entity.ToTable("merchants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.PosAccountId).IsRequired();
            entity.Property(x => x.PosProvider).IsRequired();
            entity.HasIndex(x => x.PosAccountId);
            
            entity.HasOne(x => x.Customization)
                .WithOne()
                .HasForeignKey<MerchantCustomizationEntity>(x => x.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Merchant customization
        modelBuilder.Entity<MerchantCustomizationEntity>(entity =>
        {
            entity.ToTable("merchant_customizations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MerchantId).IsRequired();
            entity.HasIndex(x => x.MerchantId).IsUnique();
        });

        // Receipt
        modelBuilder.Entity<DigitalReceiptEntity>(entity =>
        {
            entity.ToTable("receipts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TransactionId).IsRequired();
            entity.Property(x => x.Currency).IsRequired();
            entity.HasIndex(x => x.MerchantId);
            entity.HasIndex(x => x.CustomerId);
            entity.HasIndex(x => x.TransactionId);
            entity.HasIndex(x => x.CreatedAtUtc);
            
            entity.HasMany(x => x.Items)
                .WithOne()
                .HasForeignKey(x => x.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.Envelope)
                .WithOne()
                .HasForeignKey<ReceiptEnvelopeEntity>(x => x.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.Claimability)
                .WithOne()
                .HasForeignKey<ReceiptClaimabilityEntity>(x => x.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.ComplianceProfile)
                .WithOne()
                .HasForeignKey<ReceiptComplianceProfileEntity>(x => x.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.Verification)
                .WithOne()
                .HasForeignKey<ReceiptVerificationEntity>(x => x.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Receipt line items
        modelBuilder.Entity<ReceiptLineItemEntity>(entity =>
        {
            entity.ToTable("receipt_line_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
            entity.HasIndex(x => x.ReceiptId);
        });

        // Receipt envelope
        modelBuilder.Entity<ReceiptEnvelopeEntity>(entity =>
        {
            entity.ToTable("receipt_envelopes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReceiptId).IsRequired();
            entity.HasIndex(x => x.ReceiptId).IsUnique();
            
            entity.HasOne(x => x.LoyaltyModule)
                .WithOne()
                .HasForeignKey<ReceiptLoyaltyModuleEntity>(x => x.EnvelopeId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.WarrantyModule)
                .WithOne()
                .HasForeignKey<ReceiptWarrantyModuleEntity>(x => x.EnvelopeId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(x => x.AdModules)
                .WithOne()
                .HasForeignKey(x => x.EnvelopeId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(x => x.IntegrationReferences)
                .WithOne()
                .HasForeignKey(x => x.EnvelopeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Loyalty module
        modelBuilder.Entity<ReceiptLoyaltyModuleEntity>(entity =>
        {
            entity.ToTable("receipt_loyalty_modules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EnvelopeId).IsRequired();
            entity.HasIndex(x => x.EnvelopeId).IsUnique();
        });

        // Warranty module
        modelBuilder.Entity<ReceiptWarrantyModuleEntity>(entity =>
        {
            entity.ToTable("receipt_warranty_modules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EnvelopeId).IsRequired();
            entity.HasIndex(x => x.EnvelopeId).IsUnique();
        });

        // Ad module
        modelBuilder.Entity<ReceiptAdModuleEntity>(entity =>
        {
            entity.ToTable("receipt_ad_modules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EnvelopeId).IsRequired();
            entity.HasIndex(x => x.EnvelopeId);
        });

        // Integration references
        modelBuilder.Entity<ReceiptIntegrationReferenceEntity>(entity =>
        {
            entity.ToTable("receipt_integration_references");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EnvelopeId).IsRequired();
            entity.Property(x => x.IntegrationName).IsRequired();
            entity.HasIndex(x => x.EnvelopeId);
            entity.HasIndex(x => x.IntegrationName);
        });

        // Customer consent
        modelBuilder.Entity<CustomerConsentEntity>(entity =>
        {
            entity.ToTable("customer_consents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CustomerId).IsRequired();
            entity.Property(x => x.ConsentType).IsRequired();
            entity.HasIndex(x => x.CustomerId);
            entity.HasIndex(x => new { x.CustomerId, x.ConsentType, x.RevokedAtUtc });
        });

        // Customer contact
        modelBuilder.Entity<CustomerContactEntity>(entity =>
        {
            entity.ToTable("customer_contacts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CustomerId).IsRequired();
            entity.HasIndex(x => x.CustomerId).IsUnique();
            entity.HasIndex(x => x.Email);
            entity.HasIndex(x => x.Phone);
        });

        // Receipt claimability
        modelBuilder.Entity<ReceiptClaimabilityEntity>(entity =>
        {
            entity.ToTable("receipt_claimabilities");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReceiptId).IsRequired();
            entity.Property(x => x.State).IsRequired();
            entity.HasIndex(x => x.ReceiptId).IsUnique();
            entity.HasIndex(x => x.State);
            entity.HasIndex(x => x.ClaimableAtUtc);
        });

        // Receipt access logs
        modelBuilder.Entity<ReceiptAccessLogEntity>(entity =>
        {
            entity.ToTable("receipt_access_logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReceiptId).IsRequired();
            entity.Property(x => x.AccessType).IsRequired();
            entity.HasIndex(x => x.ReceiptId);
            entity.HasIndex(x => x.CustomerId);
            entity.HasIndex(x => x.AccessedAtUtc);
        });

        // Receipt rate limits
        modelBuilder.Entity<ReceiptRateLimitEntity>(entity =>
        {
            entity.ToTable("receipt_rate_limits");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Identifier).IsRequired();
            entity.Property(x => x.ResourceType).IsRequired();
            entity.HasIndex(x => new { x.Identifier, x.ResourceType, x.WindowStartUtc });
        });

        // Compliance profiles
        modelBuilder.Entity<ReceiptComplianceProfileEntity>(entity =>
        {
            entity.ToTable("receipt_compliance_profiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReceiptId).IsRequired();
            entity.HasIndex(x => x.ReceiptId).IsUnique();
            entity.HasIndex(x => x.Region);
        });

        // Receipt verification
        modelBuilder.Entity<ReceiptVerificationEntity>(entity =>
        {
            entity.ToTable("receipt_verifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReceiptId).IsRequired();
            entity.HasIndex(x => x.ReceiptId).IsUnique();
            entity.HasIndex(x => x.VerificationHash);
        });
    }
}

// ============================================================================
// CORE ENTITIES
// ============================================================================

public sealed class MerchantPortalConnectionEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PosAccountId { get; set; } = string.Empty;
    public string PosProvider { get; set; } = "square";
    public bool WebhookRegistered { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
    
    // Navigation properties
    public MerchantCustomizationEntity? Customization { get; set; }
}

public sealed class DigitalReceiptEntity
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string? OrderId { get; set; }
    public decimal Total { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? TipAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? CustomerId { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTimeOffset TransactionTimestampUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
    
    // Navigation properties
    public List<ReceiptLineItemEntity> Items { get; set; } = [];
    public ReceiptEnvelopeEntity? Envelope { get; set; }
    public ReceiptClaimabilityEntity? Claimability { get; set; }
    public ReceiptComplianceProfileEntity? ComplianceProfile { get; set; }
    public ReceiptVerificationEntity? Verification { get; set; }
}

public sealed class ReceiptLineItemEntity
{
    public int Id { get; set; }
    public Guid ReceiptId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public string? Sku { get; set; }
    public string? Category { get; set; }
}

// ============================================================================
// MERCHANT CUSTOMIZATION
// ============================================================================

public sealed class MerchantCustomizationEntity
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string? LogoUrl { get; set; }
    public string? BrandColor { get; set; }
    public string? CustomMessage { get; set; }
    public string? FooterText { get; set; }
    public bool ShowLoyaltyModule { get; set; }
    public bool ShowWarrantyModule { get; set; }
    public bool ShowAdModule { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}

// ============================================================================
// RECEIPT ENVELOPE AND MODULES
// ============================================================================

public sealed class ReceiptEnvelopeEntity
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public string EnvelopeVersion { get; set; } = "1.0";
    public DateTimeOffset CreatedAtUtc { get; set; }
    
    // Navigation properties
    public ReceiptLoyaltyModuleEntity? LoyaltyModule { get; set; }
    public ReceiptWarrantyModuleEntity? WarrantyModule { get; set; }
    public List<ReceiptAdModuleEntity> AdModules { get; set; } = [];
    public List<ReceiptIntegrationReferenceEntity> IntegrationReferences { get; set; } = [];
}

public sealed class ReceiptLoyaltyModuleEntity
{
    public Guid Id { get; set; }
    public Guid EnvelopeId { get; set; }
    public int? PointsEarned { get; set; }
    public int? PointsBalance { get; set; }
    public string? CampaignId { get; set; }
    public string? PromotionText { get; set; }
    public string? MetadataJson { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class ReceiptWarrantyModuleEntity
{
    public Guid Id { get; set; }
    public Guid EnvelopeId { get; set; }
    public string? ProductReference { get; set; }
    public string? WarrantyTermsUrl { get; set; }
    public DateTimeOffset? WarrantyExpiresAtUtc { get; set; }
    public string? RegistrationUrl { get; set; }
    public string? MetadataJson { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class ReceiptAdModuleEntity
{
    public Guid Id { get; set; }
    public Guid EnvelopeId { get; set; }
    public string AdType { get; set; } = string.Empty;
    public string? AdContent { get; set; }
    public string? AdImageUrl { get; set; }
    public string? TargetUrl { get; set; }
    public string? PlacementId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class ReceiptIntegrationReferenceEntity
{
    public Guid Id { get; set; }
    public Guid EnvelopeId { get; set; }
    public string IntegrationName { get; set; } = string.Empty;
    public string? ExternalReferenceId { get; set; }
    public string? ProvenanceMetadataJson { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

// ============================================================================
// CONSENT AND CONTACT MANAGEMENT
// ============================================================================

public sealed class CustomerConsentEntity
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string ConsentType { get; set; } = string.Empty; // email, sms, marketing, support
    public string? ConsentPurpose { get; set; }
    public string? ConsentVersion { get; set; }
    public DateTimeOffset GrantedAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public string? Scope { get; set; }
}

public sealed class CustomerContactEntity
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}

// ============================================================================
// ACCESS CONTROL AND SECURITY
// ============================================================================

public sealed class ReceiptClaimabilityEntity
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public string State { get; set; } = "pending"; // pending, claimable, claimed
    public DateTimeOffset? ClaimableAtUtc { get; set; }
    public DateTimeOffset? ClaimedAtUtc { get; set; }
    public string? ClaimMethod { get; set; } // nfc, qr, manual
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}

public sealed class ReceiptAccessLogEntity
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public string? CustomerId { get; set; }
    public string AccessType { get; set; } = string.Empty; // view, claim, retrieve
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Successful { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset AccessedAtUtc { get; set; }
}

public sealed class ReceiptRateLimitEntity
{
    public Guid Id { get; set; }
    public string Identifier { get; set; } = string.Empty; // customer_id or ip_address
    public string ResourceType { get; set; } = string.Empty; // receipt_access, history_lookup
    public int RequestCount { get; set; }
    public DateTimeOffset WindowStartUtc { get; set; }
    public DateTimeOffset WindowEndUtc { get; set; }
}

// ============================================================================
// COMPLIANCE AND VERIFICATION
// ============================================================================

public sealed class ReceiptComplianceProfileEntity
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public string Region { get; set; } = string.Empty; // US, EU, UK, etc.
    public string? TaxJurisdiction { get; set; }
    public string? ComplianceMetadataJson { get; set; }
    public bool RetentionRequired { get; set; }
    public DateTimeOffset? RetentionExpiresAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class ReceiptVerificationEntity
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public string? VerificationHash { get; set; }
    public string? SignatureData { get; set; }
    public string? SignatureAlgorithm { get; set; }
    public DateTimeOffset? SignedAtUtc { get; set; }
    public bool IsVerified { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
