﻿namespace QRSPortal2.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CircProAddresses",
                c => new
                    {
                        AddressID = c.Int(nullable: false, identity: true),
                        UserID = c.String(maxLength: 128),
                        AccountID = c.String(maxLength: 128),
                        AddressType = c.String(maxLength: 5),
                        EmailAddress = c.String(maxLength: 50),
                        AddressLine1 = c.String(maxLength: 100),
                        AddressLine2 = c.String(maxLength: 100),
                        CityTown = c.String(maxLength: 50),
                        StateParish = c.String(maxLength: 50),
                        ZipCode = c.String(maxLength: 10),
                        CountryCode = c.String(maxLength: 50),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AddressID)
                .ForeignKey("dbo.CircproUsers", t => t.UserID)
                .Index(t => t.UserID);
            
            CreateTable(
                "dbo.CircproUsers",
                c => new
                    {
                        UserID = c.String(nullable: false, maxLength: 128),
                        Id = c.Int(nullable: false, identity: true),
                        AccountID = c.String(maxLength: 10),
                        DistributionID = c.Int(nullable: false),
                        EmailAddress = c.String(maxLength: 50),
                        FirstName = c.String(maxLength: 50),
                        LastName = c.String(maxLength: 100),
                        Company = c.String(maxLength: 100),
                        PhoneNumber = c.String(maxLength: 500),
                        CellNumber = c.String(maxLength: 50),
                        IpAddress = c.String(maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        AddressID = c.Int(),
                        CreatedAt = c.DateTime(nullable: false),
                        LastLogin = c.DateTime(),
                    })
                .PrimaryKey(t => t.UserID)
                .ForeignKey("dbo.AspNetUsers", t => t.UserID)
                .Index(t => t.UserID)
                .Index(t => t.Id, unique: true);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.CircProTransactions",
                c => new
                    {
                        CircProTranxID = c.Int(nullable: false, identity: true),
                        UserID = c.String(maxLength: 128),
                        AccountID = c.String(maxLength: 128),
                        EmailAddress = c.String(maxLength: 50),
                        DistributionTypeID = c.Int(nullable: false),
                        PublicationDate = c.DateTime(nullable: false),
                        DistributionAmount = c.Int(nullable: false),
                        ReturnDate = c.DateTime(),
                        ReturnAmount = c.Int(),
                        ConfirmDate = c.DateTime(),
                        ConfirmedAmount = c.Int(),
                        ConfirmReturn = c.Boolean(),
                        Status = c.String(maxLength: 10),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.CircProTranxID)
                .ForeignKey("dbo.CircproUsers", t => t.UserID)
                .Index(t => t.UserID);
            
            CreateTable(
                "dbo.QRSActivityLogs",
                c => new
                    {
                        ActivityLogID = c.Int(nullable: false, identity: true),
                        AccountID = c.String(),
                        IPAddress = c.String(),
                        LogInformation = c.String(),
                        SystemInformation = c.String(),
                        UserName = c.String(),
                        EmailAddress = c.String(),
                        PublicationDate = c.DateTime(nullable: false),
                        ReturnAmount = c.Int(nullable: false),
                        Status = c.String(maxLength: 10),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ActivityLogID);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.CircProTransactions", "UserID", "dbo.CircproUsers");
            DropForeignKey("dbo.CircProAddresses", "UserID", "dbo.CircproUsers");
            DropForeignKey("dbo.CircproUsers", "UserID", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.CircProTransactions", new[] { "UserID" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.CircproUsers", new[] { "Id" });
            DropIndex("dbo.CircproUsers", new[] { "UserID" });
            DropIndex("dbo.CircProAddresses", new[] { "UserID" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.QRSActivityLogs");
            DropTable("dbo.CircProTransactions");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.CircproUsers");
            DropTable("dbo.CircProAddresses");
        }
    }
}
