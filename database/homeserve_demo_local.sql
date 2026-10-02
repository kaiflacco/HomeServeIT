-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: localhost
-- Generation Time: Oct 02, 2026 at 01:41 AM
-- Server version: 10.4.28-MariaDB
-- PHP Version: 8.2.4

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `homeserve_demo_local`
--

-- --------------------------------------------------------

--
-- Table structure for table `ApplicationSettingAudits`
--

CREATE TABLE `ApplicationSettingAudits` (
  `AuditId` bigint(20) NOT NULL,
  `Section` varchar(32) NOT NULL,
  `ChangedFields` varchar(500) NOT NULL,
  `ActorUserId` varchar(450) NOT NULL,
  `ChangedAtUtc` datetime(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `ApplicationSettings`
--

CREATE TABLE `ApplicationSettings` (
  `Id` int(11) NOT NULL,
  `SystemName` varchar(80) NOT NULL,
  `SupportEmail` varchar(254) NOT NULL,
  `CompanyName` varchar(120) NOT NULL,
  `CompanyAddress` varchar(250) NOT NULL,
  `NotificationRefreshEnabled` tinyint(1) NOT NULL,
  `NotificationRefreshIntervalSeconds` int(11) NOT NULL,
  `AllowPublicRegistration` tinyint(1) NOT NULL,
  `UpdatedAtUtc` datetime(6) NOT NULL,
  `UpdatedByUserId` varchar(450) DEFAULT NULL,
  `ConcurrencyStamp` varchar(36) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `AspNetRoleClaims`
--

CREATE TABLE `AspNetRoleClaims` (
  `Id` int(11) NOT NULL,
  `RoleId` varchar(255) NOT NULL,
  `ClaimType` longtext DEFAULT NULL,
  `ClaimValue` longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `AspNetRoles`
--

CREATE TABLE `AspNetRoles` (
  `Id` varchar(255) NOT NULL,
  `Name` varchar(256) DEFAULT NULL,
  `NormalizedName` varchar(256) DEFAULT NULL,
  `ConcurrencyStamp` longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `AspNetRoles`
--

INSERT INTO `AspNetRoles` (`Id`, `Name`, `NormalizedName`, `ConcurrencyStamp`) VALUES
('13adff24-a04c-4ce7-8b72-13dca3840f11', 'Customer', 'CUSTOMER', '5a8ee0ad-bf79-4aba-a31e-4589d5374629'),
('3d635219-73d1-4c86-b6f1-19951d45d692', 'Administrator', 'ADMINISTRATOR', 'cbcaf492-f0dc-420a-9198-c675cb7869a1'),
('a01f1722-da61-4c0f-a917-b906db7c00d7', 'Technician', 'TECHNICIAN', '193a67ff-4c16-44f2-aed4-2453f6754f5d');

-- --------------------------------------------------------

--
-- Table structure for table `AspNetUserClaims`
--

CREATE TABLE `AspNetUserClaims` (
  `Id` int(11) NOT NULL,
  `UserId` varchar(255) NOT NULL,
  `ClaimType` longtext DEFAULT NULL,
  `ClaimValue` longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `AspNetUserLogins`
--

CREATE TABLE `AspNetUserLogins` (
  `LoginProvider` varchar(128) NOT NULL,
  `ProviderKey` varchar(128) NOT NULL,
  `ProviderDisplayName` longtext DEFAULT NULL,
  `UserId` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `AspNetUserRoles`
--

CREATE TABLE `AspNetUserRoles` (
  `UserId` varchar(255) NOT NULL,
  `RoleId` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `AspNetUserRoles`
--

INSERT INTO `AspNetUserRoles` (`UserId`, `RoleId`) VALUES
('268300cb-a810-4c34-8908-f6642bbe7714', '13adff24-a04c-4ce7-8b72-13dca3840f11'),
('3ebeca61-a657-4d43-8eb3-936b19e9f6aa', '3d635219-73d1-4c86-b6f1-19951d45d692'),
('405561f2-e6d4-4e8f-bad7-e6a7da4a4f3a', '13adff24-a04c-4ce7-8b72-13dca3840f11'),
('4585affa-5028-4480-ac9b-be408e7eeb33', '13adff24-a04c-4ce7-8b72-13dca3840f11'),
('6a927089-fcd6-4ad4-8771-82f0f5c759e8', 'a01f1722-da61-4c0f-a917-b906db7c00d7'),
('6b2ab596-6552-47a7-a10f-b35cbfa29942', '13adff24-a04c-4ce7-8b72-13dca3840f11'),
('897f5f6c-61b6-4c1e-a258-164c7a4b489c', 'a01f1722-da61-4c0f-a917-b906db7c00d7'),
('b8863534-dc73-4eef-959a-29938aebd457', '3d635219-73d1-4c86-b6f1-19951d45d692'),
('c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'a01f1722-da61-4c0f-a917-b906db7c00d7'),
('e3608ff2-f7f6-4102-bc29-5e3dcb5dffdc', '13adff24-a04c-4ce7-8b72-13dca3840f11');

-- --------------------------------------------------------

--
-- Table structure for table `AspNetUsers`
--

CREATE TABLE `AspNetUsers` (
  `Id` varchar(255) NOT NULL,
  `FullName` varchar(100) NOT NULL,
  `StreetAddress` varchar(200) DEFAULT NULL,
  `BarangayCity` varchar(100) DEFAULT NULL,
  `Landmark` varchar(100) DEFAULT NULL,
  `ContactMethod` varchar(20) DEFAULT NULL,
  `PrefApptReminders` tinyint(1) NOT NULL,
  `PrefQuotations` tinyint(1) NOT NULL,
  `PrefInvoices` tinyint(1) NOT NULL,
  `DateCreated` datetime(6) NOT NULL,
  `LastLogin` datetime(6) DEFAULT NULL,
  `IsArchived` tinyint(1) NOT NULL,
  `Role` varchar(20) NOT NULL,
  `UserName` varchar(256) DEFAULT NULL,
  `NormalizedUserName` varchar(256) DEFAULT NULL,
  `Email` varchar(256) DEFAULT NULL,
  `NormalizedEmail` varchar(256) DEFAULT NULL,
  `EmailConfirmed` tinyint(1) NOT NULL,
  `PasswordHash` longtext DEFAULT NULL,
  `SecurityStamp` longtext DEFAULT NULL,
  `ConcurrencyStamp` longtext DEFAULT NULL,
  `PhoneNumber` longtext DEFAULT NULL,
  `PhoneNumberConfirmed` tinyint(1) NOT NULL,
  `TwoFactorEnabled` tinyint(1) NOT NULL,
  `LockoutEnd` datetime(6) DEFAULT NULL,
  `LockoutEnabled` tinyint(1) NOT NULL,
  `AccessFailedCount` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `AspNetUsers`
--

INSERT INTO `AspNetUsers` (`Id`, `FullName`, `StreetAddress`, `BarangayCity`, `Landmark`, `ContactMethod`, `PrefApptReminders`, `PrefQuotations`, `PrefInvoices`, `DateCreated`, `LastLogin`, `IsArchived`, `Role`, `UserName`, `NormalizedUserName`, `Email`, `NormalizedEmail`, `EmailConfirmed`, `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp`, `PhoneNumber`, `PhoneNumberConfirmed`, `TwoFactorEnabled`, `LockoutEnd`, `LockoutEnabled`, `AccessFailedCount`) VALUES
('268300cb-a810-4c34-8908-f6642bbe7714', 'Maria Santos', 'Buhangin, Davao City', 'Davao City', NULL, NULL, 1, 1, 1, '2026-09-25 01:22:38.711118', NULL, 0, 'Customer', 'maria@homeserveit.local', 'MARIA@HOMESERVEIT.LOCAL', 'maria@homeserveit.local', 'MARIA@HOMESERVEIT.LOCAL', 1, 'AQAAAAIAAYagAAAAEEz96Xohiw5pwl8s3E6hJgV+iTmKQXGhzicnhN+bc/ZN1qwWKURh+SSqumweaI/8vQ==', 'HAKQJXBFZIKFXACXDAUVT7LUEZY4ORUN', '2167bef0-c9c2-4e30-92a4-c020b86768b3', '+639251234567', 0, 0, NULL, 1, 0),
('3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'HomeServe Administrator', 'Davao City', 'Davao City', NULL, NULL, 1, 1, 1, '2026-09-25 01:14:15.869174', '2026-10-01 22:31:44.295439', 0, 'Administrator', 'admin@homeserveit.local', 'ADMIN@HOMESERVEIT.LOCAL', 'admin@homeserveit.local', 'ADMIN@HOMESERVEIT.LOCAL', 1, 'AQAAAAIAAYagAAAAEJ2hUqQYqbbgDFG2IK4/iHjW84m5LdA/cj4gXTTBKOHx7CUU6vt9P1CeUJpUPYI1mg==', 'TYS4WYFD5SATKJ2EHPNPZIL32ERTUDJ4', 'f5aa7973-91dc-42c2-bded-5fac5488fb92', '+639171234567', 0, 0, NULL, 1, 0),
('405561f2-e6d4-4e8f-bad7-e6a7da4a4f3a', 'Liza Navarro', 'Lanang, Davao City', 'Davao City', NULL, NULL, 1, 1, 1, '2026-09-25 01:22:38.915699', NULL, 0, 'Customer', 'liza@homeserveit.local', 'LIZA@HOMESERVEIT.LOCAL', 'liza@homeserveit.local', 'LIZA@HOMESERVEIT.LOCAL', 1, 'AQAAAAIAAYagAAAAEP3k7S0ltWgO0qtBAPjg8Leut9q3KPC5gjvbxSnxWhpio4S0GKtEMAs+pn+3PibNMA==', '3PUTVYT4QMTJ7IKEJ7X2HBBY7IVZJV6S', '28d3caef-83a8-48e0-926a-c41cab1f4c68', '+639271234567', 0, 0, NULL, 1, 0),
('4585affa-5028-4480-ac9b-be408e7eeb33', 'Kyle Cabanig', 'J.P. Laurel Avenue, Davao City', 'Davao City', NULL, NULL, 1, 1, 1, '2026-09-25 01:14:16.278406', '2026-09-25 01:26:41.474638', 0, 'Customer', 'kyle@homeserveit.local', 'KYLE@HOMESERVEIT.LOCAL', 'kyle@homeserveit.local', 'KYLE@HOMESERVEIT.LOCAL', 1, 'AQAAAAIAAYagAAAAEG79qcVDLxMmnL8u1fYNut1+1Lp7ke0Oo7o2hLfO4WKs7S3JaOTdISS4u3tdSuRBVQ==', 'EIE6DSPBCLPTVAC634ZV253X2N56MUPI', '751ad084-9209-4471-8011-d9da52db64b4', '+639191234567', 0, 0, NULL, 1, 0),
('6a927089-fcd6-4ad4-8771-82f0f5c759e8', 'Jordan Cruz', 'Matina, Davao City', 'Davao City', NULL, NULL, 1, 1, 1, '2026-09-25 01:22:38.661182', '2026-10-01 21:53:42.872412', 0, 'Technician', 'jordan@homeserveit.local', 'JORDAN@HOMESERVEIT.LOCAL', 'jordan@homeserveit.local', 'JORDAN@HOMESERVEIT.LOCAL', 1, 'AQAAAAIAAYagAAAAEHyrhVQdlNPk9LOVm42CCPTdT5auVH4Hr6O9aGzO+AQl83/7NpORmcl0enk+f5Tzqw==', 'VSQQHXDS6N65NTLFGW6NY5P2UWWKOFIO', '52cf9c34-16bb-4dec-b8d2-2dae919a3258', '+639241234567', 0, 0, NULL, 1, 0),
('6b2ab596-6552-47a7-a10f-b35cbfa29942', 'Ruben Dela Cruz', 'Matina, Davao City', 'Davao City', NULL, NULL, 1, 1, 1, '2026-09-25 01:22:38.814532', NULL, 0, 'Customer', 'ruben@homeserveit.local', 'RUBEN@HOMESERVEIT.LOCAL', 'ruben@homeserveit.local', 'RUBEN@HOMESERVEIT.LOCAL', 1, 'AQAAAAIAAYagAAAAEI96nZXg6QpqI+zhM5u96ISfmZ46p6HIdwuMFRLczmAtB3+lmX8AO9N7XNodc1Xifg==', 'QUTNO2LEDHA5RNAIXRW2277NFUONAV67', 'd3f0f1e7-58f5-4d0e-9202-48c09bd17d1c', '+639261234567', 0, 0, NULL, 1, 0),
('897f5f6c-61b6-4c1e-a258-164c7a4b489c', 'Marco Santos', 'Bajada, Davao City', 'Davao City', NULL, NULL, 1, 1, 1, '2026-09-25 01:14:16.093862', '2026-09-25 01:27:20.670530', 0, 'Technician', 'marco@homeserveit.local', 'MARCO@HOMESERVEIT.LOCAL', 'marco@homeserveit.local', 'MARCO@HOMESERVEIT.LOCAL', 1, 'AQAAAAIAAYagAAAAEL21VabLHmUqF3VyesuVT8JJnzl5YT7yA8RZzNVFvS8VTxGsstkdY8E5I0vy9wHlJQ==', '43ERWL4QRRYJB3PRYZ6RVQNEXARTWLYB', '986624ae-c81d-440c-bf94-8eaf0cafafd1', '+639181234567', 0, 0, NULL, 1, 0),
('b8863534-dc73-4eef-959a-29938aebd457', 'Sofia Reyes', 'Poblacion, Davao City', 'Davao City', NULL, NULL, 1, 1, 1, '2026-09-25 01:22:38.339204', NULL, 0, 'Administrator', 'sofia@homeserveit.local', 'SOFIA@HOMESERVEIT.LOCAL', 'sofia@homeserveit.local', 'SOFIA@HOMESERVEIT.LOCAL', 1, 'AQAAAAIAAYagAAAAEP6ufsL+vzKp8tcQAG7A1A3D63bAgh/ZT2PenJejwST88Xts8LPpasvvpsfY+s6uog==', 'VD4X5DSQKLQDOSKZ5G672L4ECEBCFUXC', '742097e7-e8ec-4ed1-b553-4a3b2216c07f', '+639221234567', 0, 0, NULL, 1, 0),
('c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Aisha Lim', 'Buhangin, Davao City', 'Davao City', NULL, NULL, 1, 1, 1, '2026-09-25 01:22:38.532063', NULL, 0, 'Technician', 'aisha@homeserveit.local', 'AISHA@HOMESERVEIT.LOCAL', 'aisha@homeserveit.local', 'AISHA@HOMESERVEIT.LOCAL', 1, 'AQAAAAIAAYagAAAAEAqlvGMP4kTR28EbD/l8l++M6ldRI691KFUWF11YtpBoQdlIrgzSSaqErl8Q7ESapA==', 'SBK4AB5BFNXTHLHB5KHMQCJAK7DOBX2Z', '3bb746c6-9915-467f-b52c-f6b39bf8c75f', '+639231234567', 0, 0, NULL, 1, 0),
('e3608ff2-f7f6-4102-bc29-5e3dcb5dffdc', 'Rina Villanueva', 'Buhangin', 'Davao City', NULL, NULL, 1, 1, 1, '2026-10-01 22:26:30.934630', NULL, 0, 'Customer', 'rina@homeserveit.local', 'RINA@HOMESERVEIT.LOCAL', 'rina@homeserveit.local', 'RINA@HOMESERVEIT.LOCAL', 1, NULL, 'I3U4UEWWWWWTRRQMAANRYIC3J2IY2YJ2', '0ccd5dfa-0ef6-4609-a4ac-528e56433b17', '+639000000121', 0, 0, NULL, 1, 0);

-- --------------------------------------------------------

--
-- Table structure for table `AspNetUserTokens`
--

CREATE TABLE `AspNetUserTokens` (
  `UserId` varchar(255) NOT NULL,
  `LoginProvider` varchar(128) NOT NULL,
  `Name` varchar(128) NOT NULL,
  `Value` longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `Customers`
--

CREATE TABLE `Customers` (
  `CustomerID` int(11) NOT NULL,
  `UserID` varchar(255) NOT NULL,
  `FirstName` varchar(50) NOT NULL,
  `LastName` varchar(50) NOT NULL,
  `PhoneNumber` varchar(50) NOT NULL,
  `HomeAddress` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `Customers`
--

INSERT INTO `Customers` (`CustomerID`, `UserID`, `FirstName`, `LastName`, `PhoneNumber`, `HomeAddress`) VALUES
(1, '4585affa-5028-4480-ac9b-be408e7eeb33', 'Kyle', 'Cabanig', '+639191234567', 'J.P. Laurel Avenue, Davao City, Davao City'),
(2, '268300cb-a810-4c34-8908-f6642bbe7714', 'Maria', 'Santos', '+639251234567', 'Buhangin, Davao City, Davao City'),
(3, '6b2ab596-6552-47a7-a10f-b35cbfa29942', 'Ruben', 'Dela Cruz', '+639261234567', 'Matina, Davao City, Davao City'),
(4, '405561f2-e6d4-4e8f-bad7-e6a7da4a4f3a', 'Liza', 'Navarro', '+639271234567', 'Lanang, Davao City, Davao City'),
(5, 'e3608ff2-f7f6-4102-bc29-5e3dcb5dffdc', 'Rina', 'Villanueva', '+639000000121', 'Buhangin, Davao City');

-- --------------------------------------------------------

--
-- Table structure for table `Devices`
--

CREATE TABLE `Devices` (
  `DeviceID` int(11) NOT NULL,
  `CustomerID` int(11) NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Type` varchar(50) NOT NULL,
  `SerialNumber` varchar(100) DEFAULT NULL,
  `DateAdded` datetime(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `Devices`
--

INSERT INTO `Devices` (`DeviceID`, `CustomerID`, `Name`, `Type`, `SerialNumber`, `DateAdded`) VALUES
(1, 1, 'Dell Inspiron 15', 'Laptop', 'HIT-DI15-2024', '2026-08-11 01:14:16.421763'),
(2, 1, 'Brother HL-L2375DW', 'Printer', 'HIT-BHL2375', '2026-08-26 01:14:16.421763'),
(3, 2, 'Dell Latitude 5440', 'Laptop', 'DL5440-MARIA-01', '2026-05-28 01:22:39.177531'),
(4, 2, 'Ubiquiti UniFi Dream Router', 'Network Device', 'UDR-MARIA-01', '2026-06-25 01:22:39.177531'),
(5, 3, 'Lenovo ThinkPad E14', 'Laptop', 'LNV-E14-RUBEN-01', '2026-07-01 01:22:39.177531'),
(6, 3, 'Synology DS224+', 'Network Storage', 'SYN-DS224-RUBEN-01', '2026-07-17 01:22:39.177531'),
(7, 4, 'MacBook Air M2', 'Laptop', 'MBA-M2-LIZA-01', '2026-08-02 01:22:39.177531'),
(8, 4, 'Brother MFC-L8900CDW', 'Printer', 'BTH-L8900-LIZA-01', '2026-08-05 01:22:39.177531'),
(9, 5, 'Lenovo ThinkPad E14 Gen 5', 'Laptop', 'HS-RINA-E14-2026', '2026-10-01 22:26:31.146208');

-- --------------------------------------------------------

--
-- Table structure for table `GeneratedReports`
--

CREATE TABLE `GeneratedReports` (
  `GeneratedReportID` int(11) NOT NULL,
  `GeneratedByUserID` varchar(255) NOT NULL,
  `ReportType` varchar(50) NOT NULL,
  `FileName` varchar(180) NOT NULL,
  `PeriodStartUtc` datetime(6) NOT NULL,
  `PeriodEndUtc` datetime(6) NOT NULL,
  `GeneratedAtUtc` datetime(6) NOT NULL,
  `PdfContent` longblob NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `InventoryItems`
--

CREATE TABLE `InventoryItems` (
  `ItemID` int(11) NOT NULL,
  `ItemName` varchar(100) NOT NULL,
  `StockQuantity` int(11) NOT NULL,
  `SKU` varchar(50) NOT NULL,
  `Category` varchar(50) NOT NULL,
  `UnitCost` decimal(10,2) NOT NULL,
  `UnitPrice` decimal(10,2) NOT NULL,
  `ReorderLevel` int(11) NOT NULL,
  `IsArchived` tinyint(1) NOT NULL,
  `ImageUrl` varchar(2000) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `InventoryItems`
--

INSERT INTO `InventoryItems` (`ItemID`, `ItemName`, `StockQuantity`, `SKU`, `Category`, `UnitCost`, `UnitPrice`, `ReorderLevel`, `IsArchived`, `ImageUrl`) VALUES
(1, 'Cat6 Ethernet Cable', 35, 'NET-CAT6-001', 'Networking', 120.00, 180.00, 10, 0, NULL),
(2, '1TB Solid State Drive', 11, 'STR-SSD1T-001', 'Storage', 2800.00, 3500.00, 3, 0, NULL),
(3, 'High-Performance Thermal Paste', 18, 'HW-THERM-001', 'Hardware', 250.00, 400.00, 5, 0, NULL),
(4, '16GB DDR4 Memory Kit', 9, 'HW-DDR4-16G', 'Hardware', 1800.00, 2300.00, 3, 0, NULL),
(5, 'UniFi U6+ Access Point', 11, 'NET-U6P-001', 'Networking', 6200.00, 7800.00, 2, 0, NULL),
(6, '2TB NVMe Solid State Drive', 5, 'STR-NVME2T-001', 'Storage', 5400.00, 6900.00, 2, 0, NULL),
(7, '8-Port Gigabit Managed Switch', 11, 'NET-SW8G-001', 'Networking', 1800.00, 2400.00, 3, 0, NULL),
(8, '1200VA Line-Interactive UPS', 4, 'PWR-UPS12-001', 'Power Protection', 3300.00, 4200.00, 2, 0, NULL),
(9, 'Brother TN-3479 Black Toner', 14, 'PRT-TN3479-001', 'Printing', 2400.00, 2950.00, 4, 0, NULL),
(10, 'TP-Link Deco X50 Wi-Fi 6 Mesh Node', 2, 'DEMO-NET-MESH-2026', 'Networking', 4300.00, 6500.00, 3, 0, NULL);

-- --------------------------------------------------------

--
-- Table structure for table `Invoices`
--

CREATE TABLE `Invoices` (
  `InvoiceID` int(11) NOT NULL,
  `RequestID` int(11) NOT NULL,
  `TotalAmount` decimal(10,2) NOT NULL,
  `PaymentStatus` varchar(20) NOT NULL,
  `DateIssued` datetime(6) NOT NULL,
  `IsQuotation` tinyint(1) NOT NULL,
  `QuotationStatus` varchar(30) NOT NULL,
  `BreakdownDetails` longtext DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `Invoices`
--

INSERT INTO `Invoices` (`InvoiceID`, `RequestID`, `TotalAmount`, `PaymentStatus`, `DateIssued`, `IsQuotation`, `QuotationStatus`, `BreakdownDetails`) VALUES
(1, 1, 4200.00, 'Paid', '2026-09-09 01:14:16.421763', 0, 'Draft', 'Diagnostics ₱500; SSD replacement ₱3,500; installation and testing ₱200'),
(2, 2, 1800.00, 'Paid', '2026-09-24 01:14:16.421763', 1, 'ApprovedByAdmin', 'Network diagnosis ₱600; cable replacement ₱360; configuration and testing ₱840'),
(3, 3, 5800.00, 'Unpaid', '2026-09-25 01:14:16.421763', 1, 'PendingAdmin', 'SSD upgrade ₱3,500; installation ₱800; performance tune-up ₱1,500'),
(4, 5, 9600.00, 'Paid', '2026-08-16 01:22:39.177531', 0, 'Draft', 'Network assessment ₱800; access point ₱7,800; installation and tuning ₱1,000'),
(5, 6, 8200.00, 'Paid', '2026-07-24 01:22:39.177531', 0, 'Draft', 'Storage recovery ₱2,000; managed switch ₱2,400; UPS replacement ₱2,800; testing ₱1,000'),
(6, 7, 12050.00, 'Paid', '2026-09-21 01:22:39.177531', 1, 'Approved', 'NVMe storage upgrade ₱6,900; toner ₱2,950; installation and maintenance ₱2,200'),
(7, 8, 3400.00, 'Unpaid', '2026-09-24 01:22:39.177531', 1, 'ApprovedByAdmin', 'Managed switch ₱2,400; site setup and testing ₱1,000'),
(8, 10, 26000.00, 'Unpaid', '2026-09-25 01:22:39.177531', 1, 'PendingAdmin', 'Two access points ₱15,600; NVMe backup storage ₱6,900; encrypted deployment ₱3,500'),
(9, 13, 8350.00, 'Paid', '2026-10-01 07:00:00.000000', 0, 'Draft', 'Wi-Fi 6 mesh node ₱6,500; installation, coverage tuning, and backup verification ₱1,850'),
(10, 15, 4850.00, 'Unpaid', '2026-10-01 22:26:31.146208', 1, 'PendingAdmin', 'Guest network configuration ₱2,100; access-point optimization ₱1,900; coverage test ₱850'),
(11, 14, 4250.00, 'Paid', '2026-09-28 07:30:00.000000', 0, 'Draft', 'Network diagnostics, DHCP configuration, and workstation connectivity verification');

-- --------------------------------------------------------

--
-- Table structure for table `JobDeliverables`
--

CREATE TABLE `JobDeliverables` (
  `DeliverableID` int(11) NOT NULL,
  `RequestID` int(11) NOT NULL,
  `Phase` varchar(50) NOT NULL,
  `Description` longtext NOT NULL,
  `ImagePath` varchar(500) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `JobDeliverables`
--

INSERT INTO `JobDeliverables` (`DeliverableID`, `RequestID`, `Phase`, `Description`, `ImagePath`, `CreatedAt`) VALUES
(1, 1, 'FinalProof', 'Completed workstation repair: storage upgraded, thermal service completed, and QA checklist passed.', NULL, '2026-09-08 01:14:16.421763'),
(2, 5, 'FinalProof', 'Network refresh completed with access-point coverage test and verified backup schedule.', NULL, '2026-08-16 01:22:39.177531'),
(3, 6, 'FinalProof', 'NAS recovery completed and a sample archive was restored successfully.', NULL, '2026-07-24 01:22:39.177531'),
(4, 7, 'FinalProof', 'Storage optimization and printer maintenance completed; review the attached service summary.', NULL, '2026-09-22 01:22:39.177531'),
(5, 13, 'FinalProof', 'Mesh coverage verified in the upstairs office; backup schedule and a sample restore were checked.', NULL, '2026-10-01 06:00:00.000000');

-- --------------------------------------------------------

--
-- Table structure for table `JobInventoryUsages`
--

CREATE TABLE `JobInventoryUsages` (
  `UsageID` int(11) NOT NULL,
  `RequestID` int(11) NOT NULL,
  `ItemID` int(11) NOT NULL,
  `Quantity` int(11) NOT NULL,
  `UnitPrice` decimal(10,2) NOT NULL,
  `IsDeducted` tinyint(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `JobInventoryUsages`
--

INSERT INTO `JobInventoryUsages` (`UsageID`, `RequestID`, `ItemID`, `Quantity`, `UnitPrice`, `IsDeducted`) VALUES
(1, 1, 1, 3, 180.00, 1),
(2, 1, 2, 1, 3500.00, 1),
(3, 1, 3, 1, 400.00, 1),
(4, 2, 1, 2, 180.00, 1),
(5, 2, 3, 1, 400.00, 1),
(6, 2, 4, 1, 2300.00, 1),
(7, 3, 2, 1, 3500.00, 0),
(8, 5, 5, 1, 7800.00, 1),
(9, 6, 7, 1, 2400.00, 1),
(10, 6, 8, 1, 4200.00, 1),
(11, 7, 6, 1, 6900.00, 1),
(12, 7, 9, 1, 2950.00, 1),
(13, 8, 7, 1, 2400.00, 0),
(14, 10, 5, 2, 7800.00, 0),
(15, 10, 6, 1, 6900.00, 0),
(16, 13, 10, 1, 6500.00, 1);

-- --------------------------------------------------------

--
-- Table structure for table `ServiceMessages`
--

CREATE TABLE `ServiceMessages` (
  `MessageID` int(11) NOT NULL,
  `RequestID` int(11) NOT NULL,
  `SenderID` varchar(255) NOT NULL,
  `Content` longtext NOT NULL,
  `Timestamp` datetime(6) NOT NULL,
  `IsRead` tinyint(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `ServiceMessages`
--

INSERT INTO `ServiceMessages` (`MessageID`, `RequestID`, `SenderID`, `Content`, `Timestamp`, `IsRead`) VALUES
(1, 1, '897f5f6c-61b6-4c1e-a258-164c7a4b489c', 'The SSD was replaced and the workstation passed the full hardware and stability checks.', '2026-09-08 01:14:16.421763', 1),
(2, 2, '4585affa-5028-4480-ac9b-be408e7eeb33', 'The connection has been stable since your last adjustment. I will monitor it during the afternoon.', '2026-09-25 00:34:16.421763', 0),
(3, 5, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'The new access point is online and the backup schedule has been tested against the office NAS.', '2026-08-16 01:22:39.177531', 1),
(4, 5, '268300cb-a810-4c34-8908-f6642bbe7714', 'Thanks, the training room is now connected and the backup report came through.', '2026-08-17 01:22:39.177531', 1),
(5, 6, '6a927089-fcd6-4ad4-8771-82f0f5c759e8', 'The recovery completed successfully. I also replaced the failing UPS battery and ran a restore test.', '2026-07-24 01:22:39.177531', 1),
(6, 9, '6b2ab596-6552-47a7-a10f-b35cbfa29942', 'The freezes happen mostly when the shared drive is syncing. I can reproduce it during large uploads.', '2026-09-25 00:47:39.177531', 0),
(7, 9, '6a927089-fcd6-4ad4-8771-82f0f5c759e8', 'I’m checking the SSD health and driver logs now. I’ll update the quotation once the root cause is confirmed.', '2026-09-25 01:02:39.177531', 0),
(8, 7, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'The maintenance checklist and storage upgrade proof are ready for your review.', '2026-09-22 01:22:39.177531', 0),
(9, 15, 'e3608ff2-f7f6-4102-bc29-5e3dcb5dffdc', 'The connection drops most often during video calls after the access point reconnects.', '2026-10-01 22:01:31.146208', 0);

-- --------------------------------------------------------

--
-- Table structure for table `ServiceRequests`
--

CREATE TABLE `ServiceRequests` (
  `RequestID` int(11) NOT NULL,
  `CustomerID` int(11) NOT NULL,
  `TechID` int(11) DEFAULT NULL,
  `IssueDescription` varchar(255) NOT NULL,
  `ScheduledDate` datetime(6) NOT NULL,
  `EstimatedDeadline` datetime(6) DEFAULT NULL,
  `Status` varchar(50) NOT NULL,
  `Priority` varchar(20) NOT NULL,
  `CompletedDate` datetime(6) DEFAULT NULL,
  `ServiceCategory` varchar(50) NOT NULL,
  `ImagePath` varchar(500) DEFAULT NULL,
  `IsArchived` tinyint(1) NOT NULL,
  `Check1_Diagnostic` tinyint(1) NOT NULL,
  `Check2_Hardware` tinyint(1) NOT NULL,
  `Check3_Firmware` tinyint(1) NOT NULL,
  `Check4_QA` tinyint(1) NOT NULL,
  `Check5_Handover` tinyint(1) NOT NULL,
  `IsCancellationRequested` tinyint(1) NOT NULL,
  `CancellationReason` varchar(500) DEFAULT NULL,
  `CancellationStatus` varchar(20) DEFAULT NULL,
  `CancellationRejectReason` varchar(500) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `ServiceRequests`
--

INSERT INTO `ServiceRequests` (`RequestID`, `CustomerID`, `TechID`, `IssueDescription`, `ScheduledDate`, `EstimatedDeadline`, `Status`, `Priority`, `CompletedDate`, `ServiceCategory`, `ImagePath`, `IsArchived`, `Check1_Diagnostic`, `Check2_Hardware`, `Check3_Firmware`, `Check4_QA`, `Check5_Handover`, `IsCancellationRequested`, `CancellationReason`, `CancellationStatus`, `CancellationRejectReason`) VALUES
(1, 1, 1, 'Completed workstation repair and storage upgrade for Kyle Cabanig.', '2026-09-07 01:14:16.421763', '2026-09-08 01:14:16.421763', 'Completed', 'Normal', '2026-09-08 01:14:16.421763', 'Hardware Repair', NULL, 0, 1, 1, 1, 1, 1, 0, NULL, NULL, NULL),
(2, 1, 1, 'Office network drops connection several times each morning.', '2026-09-24 23:14:16.421763', '2026-09-25 04:14:16.421763', 'In Progress', 'Urgent', NULL, 'Network Setup', NULL, 0, 0, 0, 0, 0, 0, 0, NULL, NULL, NULL),
(3, 1, 1, 'Laptop storage upgrade and performance tune-up.', '2026-09-28 01:14:16.421763', '2026-09-29 01:14:16.421763', 'PendingAdminApproval', 'Normal', NULL, 'Hardware Repair', NULL, 0, 0, 0, 0, 0, 0, 0, NULL, NULL, NULL),
(4, 1, NULL, 'Need help setting up a secure backup routine for family photos.', '2026-10-01 01:14:16.421763', NULL, 'Pending', 'Low', NULL, 'Data Recovery', NULL, 0, 0, 0, 0, 0, 0, 0, NULL, NULL, NULL),
(5, 2, 2, 'Quarterly office network refresh and secure backup planning for Maria Santos.', '2026-08-14 01:22:39.177531', '2026-08-16 01:22:39.177531', 'Completed', 'Normal', '2026-08-16 01:22:39.177531', 'Network Setup', NULL, 0, 1, 1, 1, 1, 1, 0, NULL, NULL, NULL),
(6, 3, 3, 'NAS storage recovery and UPS replacement after repeated brownouts.', '2026-07-22 01:22:39.177531', '2026-07-24 01:22:39.177531', 'Completed', 'Urgent', '2026-07-24 01:22:39.177531', 'Data Recovery', NULL, 0, 1, 1, 1, 1, 1, 0, NULL, NULL, NULL),
(7, 4, 2, 'Printer fleet maintenance and laptop storage optimization.', '2026-09-20 01:22:39.177531', '2026-09-22 01:22:39.177531', 'PendingCustomerReview', 'Normal', NULL, 'Hardware Repair', NULL, 0, 1, 1, 1, 1, 1, 0, NULL, NULL, NULL),
(8, 2, 2, 'Add a managed switch and extend Wi-Fi coverage to the training room.', '2026-09-27 01:22:39.177531', '2026-09-28 01:22:39.177531', 'Pending', 'Normal', NULL, 'Network Setup', NULL, 0, 0, 0, 0, 0, 0, 0, NULL, NULL, NULL),
(9, 3, 3, 'Laptop freezes during large file transfers and video calls.', '2026-09-25 00:22:39.177531', '2026-09-25 05:22:39.177531', 'Diagnosing', 'Urgent', NULL, 'Hardware Repair', NULL, 0, 0, 0, 0, 0, 0, 0, NULL, NULL, NULL),
(10, 2, 3, 'Branch office backup upgrade with encrypted off-site storage.', '2026-09-30 01:22:39.177531', '2026-10-02 01:22:39.177531', 'PendingAdminApproval', 'Normal', NULL, 'Data Recovery', NULL, 0, 0, 0, 0, 0, 0, 0, NULL, NULL, NULL),
(11, 3, NULL, 'Set up a secure workstation for a new finance team member.', '2026-10-03 01:22:39.177531', NULL, 'Pending', 'Low', NULL, 'Software / OS', NULL, 0, 0, 0, 0, 0, 0, 0, NULL, NULL, NULL),
(12, 4, NULL, 'Home office camera and access-control installation.', '2026-09-13 01:22:39.177531', NULL, 'Cancelled', 'Normal', NULL, 'CCTV / Security', NULL, 1, 0, 0, 0, 0, 0, 1, 'The customer postponed the installation indefinitely.', 'Approved', NULL),
(13, 2, 2, 'Wi-Fi 6 coverage tune-up and secure backup verification for the Davao service office.', '2026-10-01 01:00:00.000000', '2026-10-01 06:00:00.000000', 'Completed', 'Normal', '2026-10-01 06:00:00.000000', 'Network Setup', NULL, 0, 1, 1, 1, 1, 1, 0, NULL, NULL, NULL),
(14, 2, 2, 'Follow-up on intermittent DHCP lease drops across the studio workstations.', '2026-09-28 05:00:00.000000', '2026-09-28 07:00:00.000000', 'Completed', 'Normal', '2026-09-28 06:30:00.000000', 'Network Setup', NULL, 0, 1, 1, 1, 1, 1, 0, NULL, NULL, NULL),
(15, 5, 2, 'Guest Wi-Fi drops during video calls in the upstairs office.', '2026-10-02 01:00:00.000000', '2026-10-02 04:00:00.000000', 'Pending', 'Urgent', NULL, 'Network Setup', NULL, 0, 0, 0, 0, 0, 0, 0, NULL, NULL, NULL),
(16, 5, NULL, 'Configure a secure guest network for the home office and family devices.', '2026-10-03 02:00:00.000000', NULL, 'Pending', 'Normal', NULL, 'Network Setup', NULL, 0, 0, 0, 0, 0, 0, 0, NULL, NULL, NULL);

-- --------------------------------------------------------

--
-- Table structure for table `StockMovements`
--

CREATE TABLE `StockMovements` (
  `MovementID` int(11) NOT NULL,
  `ItemID` int(11) NOT NULL,
  `RequestID` int(11) DEFAULT NULL,
  `MovementType` varchar(50) NOT NULL,
  `Quantity` int(11) NOT NULL,
  `UnitCost` decimal(10,2) NOT NULL,
  `UnitPrice` decimal(10,2) NOT NULL,
  `Timestamp` datetime(6) NOT NULL,
  `PerformedBy` varchar(100) NOT NULL,
  `DestinationOrSource` varchar(255) NOT NULL,
  `Notes` varchar(500) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `StockMovements`
--

INSERT INTO `StockMovements` (`MovementID`, `ItemID`, `RequestID`, `MovementType`, `Quantity`, `UnitCost`, `UnitPrice`, `Timestamp`, `PerformedBy`, `DestinationOrSource`, `Notes`) VALUES
(1, 1, NULL, 'Initial Stock', 40, 120.00, 180.00, '2026-08-31 01:14:16.421763', 'HomeServe Administrator', 'Warehouse receiving', 'Opening stock received into the Davao service warehouse.'),
(2, 2, NULL, 'Initial Stock', 12, 2800.00, 3500.00, '2026-08-31 01:14:16.421763', 'HomeServe Administrator', 'Warehouse receiving', 'Opening stock received into the Davao service warehouse.'),
(3, 3, NULL, 'Initial Stock', 20, 250.00, 400.00, '2026-08-31 01:14:16.421763', 'HomeServe Administrator', 'Warehouse receiving', 'Opening stock received into the Davao service warehouse.'),
(4, 4, NULL, 'Initial Stock', 10, 1800.00, 2300.00, '2026-08-31 01:14:16.421763', 'HomeServe Administrator', 'Warehouse receiving', 'Opening stock received into the Davao service warehouse.'),
(5, 1, 1, 'Job Usage', -3, 120.00, 180.00, '2026-09-08 01:14:16.421763', 'Marco Santos', 'Job #JOB-0001', 'Material consumed during a completed service operation.'),
(6, 2, 1, 'Job Usage', -1, 2800.00, 3500.00, '2026-09-08 01:14:16.421763', 'Marco Santos', 'Job #JOB-0001', 'Material consumed during a completed service operation.'),
(7, 3, 1, 'Job Usage', -1, 250.00, 400.00, '2026-09-08 01:14:16.421763', 'Marco Santos', 'Job #JOB-0001', 'Material consumed during a completed service operation.'),
(8, 1, 2, 'Job Usage', -2, 120.00, 180.00, '2026-09-25 00:14:16.421763', 'Marco Santos', 'Job #JOB-0002', 'Material consumed during a completed service operation.'),
(9, 3, 2, 'Job Usage', -1, 250.00, 400.00, '2026-09-25 00:14:16.421763', 'Marco Santos', 'Job #JOB-0002', 'Material consumed during a completed service operation.'),
(10, 4, 2, 'Job Usage', -1, 1800.00, 2300.00, '2026-09-25 00:14:16.421763', 'Marco Santos', 'Job #JOB-0002', 'Material consumed during a completed service operation.'),
(11, 5, NULL, 'Initial Stock', 8, 6200.00, 7800.00, '2026-05-13 01:22:39.177531', 'HomeServe Administrator', 'Warehouse receiving', 'Opening stock received into the Davao service warehouse.'),
(12, 6, NULL, 'Initial Stock', 6, 5400.00, 6900.00, '2026-05-13 01:22:39.177531', 'HomeServe Administrator', 'Warehouse receiving', 'Opening stock received into the Davao service warehouse.'),
(13, 7, NULL, 'Initial Stock', 8, 1800.00, 2400.00, '2026-05-13 01:22:39.177531', 'HomeServe Administrator', 'Warehouse receiving', 'Opening stock received into the Davao service warehouse.'),
(14, 8, NULL, 'Initial Stock', 5, 3300.00, 4200.00, '2026-05-13 01:22:39.177531', 'HomeServe Administrator', 'Warehouse receiving', 'Opening stock received into the Davao service warehouse.'),
(15, 9, NULL, 'Initial Stock', 15, 2400.00, 2950.00, '2026-05-13 01:22:39.177531', 'HomeServe Administrator', 'Warehouse receiving', 'Opening stock received into the Davao service warehouse.'),
(16, 5, NULL, 'Restock', 4, 6200.00, 7800.00, '2026-07-29 01:22:39.177531', 'Sofia Reyes', 'Supplier delivery PO-1048', 'Stock replenishment received and counted by operations.'),
(17, 7, NULL, 'Restock', 4, 1800.00, 2400.00, '2026-08-25 01:22:39.177531', 'Sofia Reyes', 'Supplier delivery PO-1061', 'Stock replenishment received and counted by operations.'),
(18, 5, 5, 'Job Usage', -1, 6200.00, 7800.00, '2026-08-16 01:22:39.177531', 'Aisha Lim', 'Job #JOB-0005', 'Material consumed during a completed service operation.'),
(19, 7, 6, 'Job Usage', -1, 1800.00, 2400.00, '2026-07-24 01:22:39.177531', 'Jordan Cruz', 'Job #JOB-0006', 'Material consumed during a completed service operation.'),
(20, 8, 6, 'Job Usage', -1, 3300.00, 4200.00, '2026-07-24 01:22:39.177531', 'Jordan Cruz', 'Job #JOB-0006', 'Material consumed during a completed service operation.'),
(21, 6, 7, 'Job Usage', -1, 5400.00, 6900.00, '2026-09-22 01:22:39.177531', 'Aisha Lim', 'Job #JOB-0007', 'Material consumed during a completed service operation.'),
(22, 9, 7, 'Job Usage', -1, 2400.00, 2950.00, '2026-09-22 01:22:39.177531', 'Aisha Lim', 'Job #JOB-0007', 'Material consumed during a completed service operation.'),
(23, 10, NULL, 'Initial Stock', 3, 4300.00, 6500.00, '2026-09-29 22:26:31.146208', 'HomeServe Administrator', 'Warehouse receiving', 'Opening stock received into the Davao service warehouse.'),
(24, 10, 13, 'Job Usage', -1, 4300.00, 6500.00, '2026-10-01 06:00:00.000000', 'Aisha Lim', 'Job #JOB-0013', 'Material consumed during a completed service operation.');

-- --------------------------------------------------------

--
-- Table structure for table `SupportTickets`
--

CREATE TABLE `SupportTickets` (
  `SupportTicketID` int(11) NOT NULL,
  `CustomerID` int(11) NOT NULL,
  `Subject` varchar(120) NOT NULL,
  `Message` varchar(2000) NOT NULL,
  `AdminResponse` varchar(2000) DEFAULT NULL,
  `Status` varchar(50) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `RespondedAt` datetime(6) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `SupportTickets`
--

INSERT INTO `SupportTickets` (`SupportTicketID`, `CustomerID`, `Subject`, `Message`, `AdminResponse`, `Status`, `CreatedAt`, `RespondedAt`) VALUES
(1, 1, 'Question about service warranty', 'Please confirm how long the replacement SSD is covered after the completed repair.', NULL, 'Open', '2026-09-23 01:14:16.421763', NULL),
(2, 2, 'Request for monthly backup report', 'Could we receive a monthly confirmation that the branch backup is completing successfully?', NULL, 'Open', '2026-09-24 01:22:39.177531', NULL),
(3, 3, 'Follow-up on workstation freezes', 'The issue returned briefly this morning while the shared drive was syncing.', 'Jordan is investigating the storage health and will update the active request.', 'In Progress', '2026-09-24 20:22:39.177531', '2026-09-24 21:22:39.177531'),
(4, 4, 'Invoice copy requested', 'Please send a copy of the completed maintenance invoice for our records.', 'A copy of the invoice is available from Bills & Payments.', 'Resolved', '2026-09-16 01:22:39.177531', '2026-09-17 01:22:39.177531'),
(5, 5, 'Help checking upstairs Wi-Fi coverage', 'Could you include the upstairs office in the coverage check during the scheduled visit?', NULL, 'Open', '2026-10-01 22:11:31.146208', NULL);

-- --------------------------------------------------------

--
-- Table structure for table `Technicians`
--

CREATE TABLE `Technicians` (
  `TechID` int(11) NOT NULL,
  `UserID` varchar(255) NOT NULL,
  `FirstName` varchar(50) NOT NULL,
  `LastName` varchar(50) NOT NULL,
  `Specialty` varchar(255) NOT NULL,
  `IsAvailable` tinyint(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `Technicians`
--

INSERT INTO `Technicians` (`TechID`, `UserID`, `FirstName`, `LastName`, `Specialty`, `IsAvailable`) VALUES
(1, '897f5f6c-61b6-4c1e-a258-164c7a4b489c', 'Marco', 'Santos', 'Hardware and network support', 1),
(2, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Aisha', 'Lim', 'Network and security systems', 1),
(3, '6a927089-fcd6-4ad4-8771-82f0f5c759e8', 'Jordan', 'Cruz', 'Hardware and data recovery', 1);

-- --------------------------------------------------------

--
-- Table structure for table `UserNotifications`
--

CREATE TABLE `UserNotifications` (
  `NotificationID` int(11) NOT NULL,
  `RecipientUserID` varchar(255) NOT NULL,
  `AudienceRole` varchar(32) NOT NULL,
  `Category` varchar(40) NOT NULL,
  `Icon` varchar(40) NOT NULL,
  `Title` varchar(180) NOT NULL,
  `Message` varchar(600) NOT NULL,
  `ActionUrl` varchar(300) NOT NULL,
  `SourceKey` varchar(200) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `IsRead` tinyint(1) NOT NULL,
  `ReadAt` datetime(6) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `UserNotifications`
--

INSERT INTO `UserNotifications` (`NotificationID`, `RecipientUserID`, `AudienceRole`, `Category`, `Icon`, `Title`, `Message`, `ActionUrl`, `SourceKey`, `CreatedAt`, `IsRead`, `ReadAt`) VALUES
(1, '4585affa-5028-4480-ac9b-be408e7eeb33', 'Customer', 'Jobs', 'wrench', 'Technician is working on your network request', 'Marco Santos is currently handling your office network issue.', '/Customer/ServiceRequests?jobId=2', 'operations:customer:active-job:2', '2026-09-25 00:14:16.421763', 0, NULL),
(2, '3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'Administrator', 'Quotations', 'file-text', 'Quotation awaiting review', 'A hardware-upgrade quotation is ready for administrator review.', '/Admin/Finance/Quotations#3', 'operations:admin:quotation:3', '2026-09-25 01:14:16.421763', 1, '2026-09-25 01:25:53.848542'),
(3, '3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'Administrator', 'Jobs', 'clipboard', 'Service request needs a technician', 'JOB-0004 for Kyle Cabanig is scheduled for Oct 1, 2026 at 1:14 AM.', '/Admin/Operations/ServiceRequests?requestId=4', 'admin:request:4:pending', '2026-09-25 01:15:39.258238', 1, '2026-09-25 01:25:53.848542'),
(4, '3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'Administrator', 'Quotations', 'file-text', 'Quotation needs approval', 'QT-0003 for JOB-0003 totals ₱5,800.00 and is waiting for admin review.', '/Admin/Finance/Quotations', 'admin:quotation:3:pending-admin', '2026-09-25 01:15:39.258238', 1, '2026-09-25 01:25:53.848542'),
(5, '3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'Administrator', 'Support', 'message-circle', 'New customer support request', 'Kyle Cabanig: Question about service warranty', '/Admin/Support?status=All&ticketId=1', 'admin:support:1:open', '2026-09-25 01:15:39.258238', 1, '2026-09-25 01:25:53.848542'),
(6, '4585affa-5028-4480-ac9b-be408e7eeb33', 'Customer', 'Jobs', 'wrench', 'Service request received', 'JOB-0004 is scheduled for Oct 1, 2026 at 1:14 AM. A technician will be assigned soon.', '/Customer/ServiceRequests?jobId=4', 'customer:request:4:pending:none', '2026-09-25 01:15:39.457752', 0, NULL),
(7, '4585affa-5028-4480-ac9b-be408e7eeb33', 'Customer', 'Jobs', 'wrench', 'Service request updated to PendingAdminApproval', 'JOB-0003 is scheduled for Sep 28, 2026 at 1:14 AM. Assigned technician: Marco Santos.', '/Customer/ServiceRequests?jobId=3', 'customer:request:3:pendingadminapproval:none', '2026-09-25 01:15:39.457752', 0, NULL),
(8, '4585affa-5028-4480-ac9b-be408e7eeb33', 'Customer', 'Jobs', 'wrench', 'Your service is in progress', 'JOB-0002 is scheduled for Sep 24, 2026 at 11:14 PM. Assigned technician: Marco Santos.', '/Customer/ServiceRequests?jobId=2', 'customer:request:2:in progress:none', '2026-09-25 01:15:39.457752', 0, NULL),
(9, '4585affa-5028-4480-ac9b-be408e7eeb33', 'Customer', 'Jobs', 'check-circle', 'Service request completed', 'JOB-0001 is scheduled for Sep 7, 2026 at 1:14 AM. Assigned technician: Marco Santos.', '/Customer/ServiceRequests?jobId=1', 'customer:request:1:completed:none', '2026-09-25 01:15:39.457752', 0, NULL),
(10, '4585affa-5028-4480-ac9b-be408e7eeb33', 'Customer', 'Quotations', 'file-text', 'Quotation is under admin review', 'QT-0003 for JOB-0003 totals ₱5,800.00.', '/Customer/Quotations', 'customer:quotation:3:pendingadmin', '2026-09-25 01:15:39.457752', 0, NULL),
(11, '4585affa-5028-4480-ac9b-be408e7eeb33', 'Customer', 'Quotations', 'file-text', 'Quotation ready for your review', 'QT-0002 for JOB-0002 totals ₱1,800.00.', '/Customer/Quotations', 'customer:quotation:2:approvedbyadmin', '2026-09-25 01:15:39.457752', 0, NULL),
(12, '4585affa-5028-4480-ac9b-be408e7eeb33', 'Customer', 'Billing', 'check-circle', 'Payment confirmed', 'INV-0001 for JOB-0001 totals ₱4,200.00 and is paid.', '/Customer/BillsAndPayments', 'customer:invoice:1:paid', '2026-09-25 01:15:39.457752', 0, NULL),
(13, '4585affa-5028-4480-ac9b-be408e7eeb33', 'Customer', 'Support', 'message-circle', 'Support request received', 'Your request “Question about service warranty” is open.', '/Customer/Support?ticketId=1', 'customer:support:1:open:0', '2026-09-25 01:15:39.457752', 0, NULL),
(14, '897f5f6c-61b6-4c1e-a258-164c7a4b489c', 'Technician', 'Jobs', 'tool', 'Assigned job updated to PendingAdminApproval', 'JOB-0003 for Kyle Cabanig is scheduled for Sep 28, 2026 at 1:14 AM.', '/Technician/AssignedJobs?jobId=3', 'technician:request:3:pendingadminapproval', '2026-09-25 01:15:39.517647', 0, NULL),
(15, '897f5f6c-61b6-4c1e-a258-164c7a4b489c', 'Technician', 'Jobs', 'tool', 'Job is marked in progress', 'JOB-0002 for Kyle Cabanig is scheduled for Sep 24, 2026 at 11:14 PM.', '/Technician/AssignedJobs?jobId=2', 'technician:request:2:in progress', '2026-09-25 01:15:39.517647', 0, NULL),
(16, '897f5f6c-61b6-4c1e-a258-164c7a4b489c', 'Technician', 'Jobs', 'check-circle', 'Job marked completed', 'JOB-0001 for Kyle Cabanig is scheduled for Sep 7, 2026 at 1:14 AM.', '/Technician/AssignedJobs?jobId=1', 'technician:request:1:completed', '2026-09-25 01:15:39.517647', 0, NULL),
(17, '897f5f6c-61b6-4c1e-a258-164c7a4b489c', 'Technician', 'Quotations', 'file-text', 'Quotation submitted for admin review', 'QT-0003 for JOB-0003 totals ₱5,800.00.', '/Technician/AssignedJobs?jobId=3', 'technician:quotation:3:pendingadmin', '2026-09-25 01:15:39.517647', 0, NULL),
(18, '897f5f6c-61b6-4c1e-a258-164c7a4b489c', 'Technician', 'Quotations', 'file-text', 'Your quotation was approved', 'QT-0002 for JOB-0002 totals ₱1,800.00.', '/Technician/AssignedJobs?jobId=2', 'technician:quotation:2:approvedbyadmin', '2026-09-25 01:15:39.517647', 0, NULL),
(19, '897f5f6c-61b6-4c1e-a258-164c7a4b489c', 'Technician', 'Billing', 'check-circle', 'Invoice payment recorded', 'Payment of ₱1,800.00 has been recorded for QT-0002 on JOB-0002. Open the assigned job to review the next step.', '/Technician/AssignedJobs?jobId=2', 'technician:invoice:2:paid', '2026-09-25 01:15:39.517647', 0, NULL),
(20, '897f5f6c-61b6-4c1e-a258-164c7a4b489c', 'Technician', 'Billing', 'check-circle', 'Invoice payment recorded', 'Payment of ₱4,200.00 has been recorded for INV-0001 on JOB-0001. Open the assigned job to review the next step.', '/Technician/AssignedJobs?jobId=1', 'technician:invoice:1:paid', '2026-09-25 01:15:39.517647', 0, NULL),
(21, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Quotations', 'file-text', 'Quotation awaiting review', 'Maria Santos has a branch backup upgrade quotation awaiting review.', '/Admin/Finance/Quotations#10', 'operations:admin:quotation:10', '2026-09-25 01:22:39.177531', 0, NULL),
(22, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Jobs', 'wrench', 'Customer approval received', 'Liza Navarro’s completed maintenance job is ready for customer review.', '/Technician/AssignedJobs?jobId=7', 'operations:technician:review:7', '2026-09-22 01:22:39.177531', 0, NULL),
(23, '6a927089-fcd6-4ad4-8771-82f0f5c759e8', 'Technician', 'Jobs', 'tool', 'Diagnosis in progress', 'Ruben Dela Cruz’s laptop issue is being investigated today.', '/Technician/AssignedJobs?jobId=9', 'operations:technician:diagnosis:9', '2026-09-25 01:02:39.177531', 0, NULL),
(24, '268300cb-a810-4c34-8908-f6642bbe7714', 'Customer', 'Billing', 'credit-card', 'Quotation ready for review', 'Your training-room network quotation has been approved and is ready for payment.', '/Customer/Quotations?requestId=8', 'operations:customer:quotation:8', '2026-09-24 01:22:39.177531', 0, NULL),
(25, '6b2ab596-6552-47a7-a10f-b35cbfa29942', 'Customer', 'Jobs', 'wrench', 'Technician update', 'Jordan Cruz is investigating the workstation issue and will share the next update soon.', '/Customer/ServiceRequests?jobId=9', 'operations:customer:diagnosis:9', '2026-09-25 01:02:39.177531', 0, NULL),
(26, '405561f2-e6d4-4e8f-bad7-e6a7da4a4f3a', 'Customer', 'Jobs', 'check-circle', 'Service proof ready', 'Your completed maintenance work is ready for review.', '/Customer/ServiceRequests?jobId=7', 'operations:customer:review:7', '2026-09-22 01:22:39.177531', 0, NULL),
(27, '268300cb-a810-4c34-8908-f6642bbe7714', 'Customer', 'Jobs', 'wrench', 'Service request updated to PendingAdminApproval', 'JOB-0010 is scheduled for Sep 30, 2026 at 1:22 AM. Assigned technician: Jordan Cruz.', '/Customer/ServiceRequests?jobId=10', 'customer:request:10:pendingadminapproval:none', '2026-09-25 01:23:05.938784', 0, NULL),
(28, '268300cb-a810-4c34-8908-f6642bbe7714', 'Customer', 'Jobs', 'wrench', 'Service request received', 'JOB-0008 is scheduled for Sep 27, 2026 at 1:22 AM. Assigned technician: Aisha Lim.', '/Customer/ServiceRequests?jobId=8', 'customer:request:8:pending:none', '2026-09-25 01:23:05.938784', 0, NULL),
(29, '268300cb-a810-4c34-8908-f6642bbe7714', 'Customer', 'Jobs', 'check-circle', 'Service request completed', 'JOB-0005 is scheduled for Aug 14, 2026 at 1:22 AM. Assigned technician: Aisha Lim.', '/Customer/ServiceRequests?jobId=5', 'customer:request:5:completed:none', '2026-09-25 01:23:05.938784', 0, NULL),
(30, '268300cb-a810-4c34-8908-f6642bbe7714', 'Customer', 'Quotations', 'file-text', 'Quotation is under admin review', 'QT-0008 for JOB-0010 totals ₱26,000.00.', '/Customer/Quotations', 'customer:quotation:8:pendingadmin', '2026-09-25 01:23:05.938784', 0, NULL),
(31, '268300cb-a810-4c34-8908-f6642bbe7714', 'Customer', 'Quotations', 'file-text', 'Quotation ready for your review', 'QT-0007 for JOB-0008 totals ₱3,400.00.', '/Customer/Quotations', 'customer:quotation:7:approvedbyadmin', '2026-09-25 01:23:05.938784', 0, NULL),
(32, '268300cb-a810-4c34-8908-f6642bbe7714', 'Customer', 'Billing', 'check-circle', 'Payment confirmed', 'INV-0004 for JOB-0005 totals ₱9,600.00 and is paid.', '/Customer/BillsAndPayments', 'customer:invoice:4:paid', '2026-09-25 01:23:05.938784', 0, NULL),
(33, '268300cb-a810-4c34-8908-f6642bbe7714', 'Customer', 'Support', 'message-circle', 'Support request received', 'Your request “Request for monthly backup report” is open.', '/Customer/Support?ticketId=2', 'customer:support:2:open:0', '2026-09-25 01:23:05.938784', 0, NULL),
(34, '3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'Administrator', 'Jobs', 'clipboard', 'Service request needs a technician', 'JOB-0011 for Ruben Dela Cruz is scheduled for Oct 3, 2026 at 1:22 AM.', '/Admin/Operations/ServiceRequests?requestId=11', 'admin:request:11:pending', '2026-09-25 01:23:06.079508', 1, '2026-09-25 01:25:53.848542'),
(35, '3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'Administrator', 'Jobs', 'clipboard', 'Service request is ready to start', 'JOB-0008 for Maria Santos is scheduled for Sep 27, 2026 at 1:22 AM.', '/Admin/Operations/ServiceRequests?requestId=8', 'admin:request:8:pending', '2026-09-25 01:23:06.079508', 1, '2026-09-25 01:25:53.848542'),
(36, '3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'Administrator', 'Quotations', 'file-text', 'Quotation needs approval', 'QT-0008 for JOB-0010 totals ₱26,000.00 and is waiting for admin review.', '/Admin/Finance/Quotations', 'admin:quotation:8:pending-admin', '2026-09-25 01:23:06.079508', 1, '2026-09-25 01:25:53.848542'),
(37, '3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'Administrator', 'Support', 'message-circle', 'Customer support request in progress', 'Ruben Dela Cruz: Follow-up on workstation freezes', '/Admin/Support?status=All&ticketId=3', 'admin:support:3:in progress', '2026-09-25 01:23:06.079508', 1, '2026-09-25 01:25:53.848542'),
(38, '3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'Administrator', 'Support', 'message-circle', 'New customer support request', 'Maria Santos: Request for monthly backup report', '/Admin/Support?status=All&ticketId=2', 'admin:support:2:open', '2026-09-25 01:23:06.079508', 1, '2026-09-25 01:25:53.848542'),
(39, '405561f2-e6d4-4e8f-bad7-e6a7da4a4f3a', 'Customer', 'Jobs', 'wrench', 'Cancellation request approved', 'JOB-0012 is scheduled for Sep 13, 2026 at 1:22 AM. A technician will be assigned soon.', '/Customer/ServiceRequests?jobId=12&filter=Cancelled', 'customer:request:12:cancelled:approved', '2026-09-25 01:23:06.089703', 0, NULL),
(40, '405561f2-e6d4-4e8f-bad7-e6a7da4a4f3a', 'Customer', 'Jobs', 'wrench', 'Service work is ready for your approval', 'JOB-0007 is scheduled for Sep 20, 2026 at 1:22 AM. Assigned technician: Aisha Lim.', '/Customer/ServiceRequests?jobId=7', 'customer:request:7:pendingcustomerreview:none', '2026-09-25 01:23:06.089703', 0, NULL),
(41, '405561f2-e6d4-4e8f-bad7-e6a7da4a4f3a', 'Customer', 'Quotations', 'file-text', 'Quotation ready for your review', 'QT-0006 for JOB-0007 totals ₱12,050.00.', '/Customer/Quotations', 'customer:quotation:6:approved', '2026-09-25 01:23:06.089703', 0, NULL),
(42, '405561f2-e6d4-4e8f-bad7-e6a7da4a4f3a', 'Customer', 'Support', 'message-circle', 'Support replied to your request', 'Your request “Invoice copy requested” has a new response and is resolved.', '/Customer/Support?ticketId=4', 'customer:support:4:resolved:639252049591775310', '2026-09-25 01:23:06.089703', 0, NULL),
(43, '6a927089-fcd6-4ad4-8771-82f0f5c759e8', 'Technician', 'Jobs', 'tool', 'Assigned job updated to PendingAdminApproval', 'JOB-0010 for Maria Santos is scheduled for Sep 30, 2026 at 1:22 AM.', '/Technician/AssignedJobs?jobId=10', 'technician:request:10:pendingadminapproval', '2026-09-25 01:23:06.116991', 0, NULL),
(44, '6a927089-fcd6-4ad4-8771-82f0f5c759e8', 'Technician', 'Jobs', 'tool', 'Diagnosis stage recorded', 'JOB-0009 for Ruben Dela Cruz is scheduled for Sep 25, 2026 at 12:22 AM.', '/Technician/AssignedJobs?jobId=9', 'technician:request:9:diagnosing', '2026-09-25 01:23:06.116991', 0, NULL),
(45, '6a927089-fcd6-4ad4-8771-82f0f5c759e8', 'Technician', 'Jobs', 'check-circle', 'Job marked completed', 'JOB-0006 for Ruben Dela Cruz is scheduled for Jul 22, 2026 at 1:22 AM.', '/Technician/AssignedJobs?jobId=6', 'technician:request:6:completed', '2026-09-25 01:23:06.116991', 0, NULL),
(46, '6a927089-fcd6-4ad4-8771-82f0f5c759e8', 'Technician', 'Quotations', 'file-text', 'Quotation submitted for admin review', 'QT-0008 for JOB-0010 totals ₱26,000.00.', '/Technician/AssignedJobs?jobId=10', 'technician:quotation:8:pendingadmin', '2026-09-25 01:23:06.116991', 0, NULL),
(47, '6a927089-fcd6-4ad4-8771-82f0f5c759e8', 'Technician', 'Billing', 'check-circle', 'Invoice payment recorded', 'Payment of ₱8,200.00 has been recorded for INV-0005 on JOB-0006. Open the assigned job to review the next step.', '/Technician/AssignedJobs?jobId=6', 'technician:invoice:5:paid', '2026-09-25 01:23:06.116991', 0, NULL),
(48, '6b2ab596-6552-47a7-a10f-b35cbfa29942', 'Customer', 'Jobs', 'wrench', 'Service request received', 'JOB-0011 is scheduled for Oct 3, 2026 at 1:22 AM. A technician will be assigned soon.', '/Customer/ServiceRequests?jobId=11', 'customer:request:11:pending:none', '2026-09-25 01:23:06.126768', 0, NULL),
(49, '6b2ab596-6552-47a7-a10f-b35cbfa29942', 'Customer', 'Jobs', 'wrench', 'Your device is being diagnosed', 'JOB-0009 is scheduled for Sep 25, 2026 at 12:22 AM. Assigned technician: Jordan Cruz.', '/Customer/ServiceRequests?jobId=9', 'customer:request:9:diagnosing:none', '2026-09-25 01:23:06.126768', 0, NULL),
(50, '6b2ab596-6552-47a7-a10f-b35cbfa29942', 'Customer', 'Jobs', 'check-circle', 'Service request completed', 'JOB-0006 is scheduled for Jul 22, 2026 at 1:22 AM. Assigned technician: Jordan Cruz.', '/Customer/ServiceRequests?jobId=6', 'customer:request:6:completed:none', '2026-09-25 01:23:06.126768', 0, NULL),
(51, '6b2ab596-6552-47a7-a10f-b35cbfa29942', 'Customer', 'Billing', 'check-circle', 'Payment confirmed', 'INV-0005 for JOB-0006 totals ₱8,200.00 and is paid.', '/Customer/BillsAndPayments', 'customer:invoice:5:paid', '2026-09-25 01:23:06.126768', 0, NULL),
(52, '6b2ab596-6552-47a7-a10f-b35cbfa29942', 'Customer', 'Support', 'message-circle', 'Support replied to your request', 'Your request “Follow-up on workstation freezes” has a new response and is in progress.', '/Customer/Support?ticketId=3', 'customer:support:3:in progress:639258817591775310', '2026-09-25 01:23:06.126768', 0, NULL),
(53, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Jobs', 'clipboard', 'Service request needs a technician', 'JOB-0011 for Ruben Dela Cruz is scheduled for Oct 3, 2026 at 1:22 AM.', '/Admin/Operations/ServiceRequests?requestId=11', 'admin:request:11:pending', '2026-09-25 01:23:06.148018', 0, NULL),
(54, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Jobs', 'clipboard', 'Service request is ready to start', 'JOB-0008 for Maria Santos is scheduled for Sep 27, 2026 at 1:22 AM.', '/Admin/Operations/ServiceRequests?requestId=8', 'admin:request:8:pending', '2026-09-25 01:23:06.148018', 0, NULL),
(55, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Jobs', 'clipboard', 'Service request needs a technician', 'JOB-0004 for Kyle Cabanig is scheduled for Oct 1, 2026 at 1:14 AM.', '/Admin/Operations/ServiceRequests?requestId=4', 'admin:request:4:pending', '2026-09-25 01:23:06.148018', 0, NULL),
(56, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Quotations', 'file-text', 'Quotation needs approval', 'QT-0008 for JOB-0010 totals ₱26,000.00 and is waiting for admin review.', '/Admin/Finance/Quotations', 'admin:quotation:8:pending-admin', '2026-09-25 01:23:06.148018', 0, NULL),
(57, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Quotations', 'file-text', 'Quotation needs approval', 'QT-0003 for JOB-0003 totals ₱5,800.00 and is waiting for admin review.', '/Admin/Finance/Quotations', 'admin:quotation:3:pending-admin', '2026-09-25 01:23:06.148018', 0, NULL),
(58, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Support', 'message-circle', 'Customer support request in progress', 'Ruben Dela Cruz: Follow-up on workstation freezes', '/Admin/Support?status=All&ticketId=3', 'admin:support:3:in progress', '2026-09-25 01:23:06.148018', 0, NULL),
(59, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Support', 'message-circle', 'New customer support request', 'Maria Santos: Request for monthly backup report', '/Admin/Support?status=All&ticketId=2', 'admin:support:2:open', '2026-09-25 01:23:06.148018', 0, NULL),
(60, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Support', 'message-circle', 'New customer support request', 'Kyle Cabanig: Question about service warranty', '/Admin/Support?status=All&ticketId=1', 'admin:support:1:open', '2026-09-25 01:23:06.148018', 0, NULL),
(61, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Jobs', 'tool', 'New job assigned to you', 'JOB-0008 for Maria Santos is scheduled for Sep 27, 2026 at 1:22 AM.', '/Technician/AssignedJobs?jobId=8', 'technician:request:8:pending', '2026-09-25 01:23:06.187279', 0, NULL),
(62, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Jobs', 'tool', 'Awaiting customer approval', 'JOB-0007 for Liza Navarro is scheduled for Sep 20, 2026 at 1:22 AM.', '/Technician/AssignedJobs?jobId=7', 'technician:request:7:pendingcustomerreview', '2026-09-25 01:23:06.187279', 0, NULL),
(63, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Jobs', 'check-circle', 'Job marked completed', 'JOB-0005 for Maria Santos is scheduled for Aug 14, 2026 at 1:22 AM.', '/Technician/AssignedJobs?jobId=5', 'technician:request:5:completed', '2026-09-25 01:23:06.187279', 0, NULL),
(64, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Quotations', 'file-text', 'Your quotation was approved', 'QT-0007 for JOB-0008 totals ₱3,400.00.', '/Technician/AssignedJobs?jobId=8', 'technician:quotation:7:approvedbyadmin', '2026-09-25 01:23:06.187279', 0, NULL),
(65, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Quotations', 'file-text', 'Your quotation was approved', 'QT-0006 for JOB-0007 totals ₱12,050.00.', '/Technician/AssignedJobs?jobId=7', 'technician:quotation:6:approved', '2026-09-25 01:23:06.187279', 0, NULL),
(66, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Billing', 'check-circle', 'Invoice payment recorded', 'Payment of ₱12,050.00 has been recorded for QT-0006 on JOB-0007. Open the assigned job to review the next step.', '/Technician/AssignedJobs?jobId=7', 'technician:invoice:6:paid', '2026-09-25 01:23:06.187279', 0, NULL),
(67, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Billing', 'check-circle', 'Invoice payment recorded', 'Payment of ₱9,600.00 has been recorded for INV-0004 on JOB-0005. Open the assigned job to review the next step.', '/Technician/AssignedJobs?jobId=5', 'technician:invoice:4:paid', '2026-09-25 01:23:06.187279', 0, NULL),
(68, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Quotations', 'file-text', 'Quotation awaiting review', 'Rina Villanueva\'s guest network quotation is ready for review.', '/Admin/Finance/Quotations#15', 'recent-activity:admin-quotation:15', '2026-10-01 22:21:31.146208', 0, NULL),
(69, 'e3608ff2-f7f6-4102-bc29-5e3dcb5dffdc', 'Customer', 'Jobs', 'calendar', 'Service visit scheduled', 'Your guest Wi-Fi coverage check is scheduled for today.', '/Customer/ServiceRequests?jobId=15', 'recent-activity:customer-schedule:15', '2026-10-01 22:23:31.146208', 0, NULL),
(70, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Jobs', 'wrench', 'New visit on today\'s schedule', 'Rina Villanueva\'s guest Wi-Fi coverage check is scheduled for today.', '/Technician/AssignedJobs?jobId=15', 'recent-activity:technician-schedule:15', '2026-10-01 22:24:31.146208', 0, NULL),
(71, '268300cb-a810-4c34-8908-f6642bbe7714', 'Customer', 'Jobs', 'check-circle', 'Service request completed', 'JOB-0014 is scheduled for Sep 28, 2026 at 5:00 AM. Assigned technician: Aisha Lim.', '/Customer/ServiceRequests?jobId=14', 'customer:request:14:completed:none', '2026-10-01 22:26:34.667099', 0, NULL),
(72, '268300cb-a810-4c34-8908-f6642bbe7714', 'Customer', 'Jobs', 'check-circle', 'Service request completed', 'JOB-0013 is scheduled for Oct 1, 2026 at 1:00 AM. Assigned technician: Aisha Lim.', '/Customer/ServiceRequests?jobId=13', 'customer:request:13:completed:none', '2026-10-01 22:26:34.667099', 0, NULL),
(73, '268300cb-a810-4c34-8908-f6642bbe7714', 'Customer', 'Billing', 'check-circle', 'Payment confirmed', 'INV-0009 for JOB-0013 totals ₱8,350.00 and is paid.', '/Customer/BillsAndPayments', 'customer:invoice:9:paid', '2026-10-01 22:26:34.667099', 0, NULL),
(74, '268300cb-a810-4c34-8908-f6642bbe7714', 'Customer', 'Billing', 'check-circle', 'Payment confirmed', 'INV-0011 for JOB-0014 totals ₱4,250.00 and is paid.', '/Customer/BillsAndPayments', 'customer:invoice:11:paid', '2026-10-01 22:26:34.667099', 0, NULL),
(75, '3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'Administrator', 'Jobs', 'clipboard', 'Service request needs a technician', 'JOB-0016 for Rina Villanueva is scheduled for Oct 3, 2026 at 2:00 AM.', '/Admin/Operations/ServiceRequests?requestId=16', 'admin:request:16:pending', '2026-10-01 22:26:34.749385', 0, NULL),
(76, '3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'Administrator', 'Jobs', 'clipboard', 'Service request is ready to start', 'JOB-0015 for Rina Villanueva is scheduled for Oct 2, 2026 at 1:00 AM.', '/Admin/Operations/ServiceRequests?requestId=15', 'admin:request:15:pending', '2026-10-01 22:26:34.749385', 0, NULL),
(77, '3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'Administrator', 'Quotations', 'file-text', 'Quotation needs approval', 'QT-0010 for JOB-0015 totals ₱4,850.00 and is waiting for admin review.', '/Admin/Finance/Quotations', 'admin:quotation:10:pending-admin', '2026-10-01 22:26:34.749385', 0, NULL),
(78, '3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'Administrator', 'Inventory', 'package', 'Inventory is at or below reorder level', 'TP-Link Deco X50 Wi-Fi 6 Mesh Node has 2 units remaining; reorder level is 3.', '/Admin/Finance/Inventory', 'admin:inventory:10:low:2', '2026-10-01 22:26:34.749385', 0, NULL),
(79, '3ebeca61-a657-4d43-8eb3-936b19e9f6aa', 'Administrator', 'Support', 'message-circle', 'New customer support request', 'Rina Villanueva: Help checking upstairs Wi-Fi coverage', '/Admin/Support?status=All&ticketId=5', 'admin:support:5:open', '2026-10-01 22:26:34.749385', 0, NULL),
(80, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Jobs', 'clipboard', 'Service request needs a technician', 'JOB-0016 for Rina Villanueva is scheduled for Oct 3, 2026 at 2:00 AM.', '/Admin/Operations/ServiceRequests?requestId=16', 'admin:request:16:pending', '2026-10-01 22:26:34.812612', 0, NULL),
(81, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Jobs', 'clipboard', 'Service request is ready to start', 'JOB-0015 for Rina Villanueva is scheduled for Oct 2, 2026 at 1:00 AM.', '/Admin/Operations/ServiceRequests?requestId=15', 'admin:request:15:pending', '2026-10-01 22:26:34.812612', 0, NULL),
(82, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Quotations', 'file-text', 'Quotation needs approval', 'QT-0010 for JOB-0015 totals ₱4,850.00 and is waiting for admin review.', '/Admin/Finance/Quotations', 'admin:quotation:10:pending-admin', '2026-10-01 22:26:34.812612', 0, NULL),
(83, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Inventory', 'package', 'Inventory is at or below reorder level', 'TP-Link Deco X50 Wi-Fi 6 Mesh Node has 2 units remaining; reorder level is 3.', '/Admin/Finance/Inventory', 'admin:inventory:10:low:2', '2026-10-01 22:26:34.812612', 0, NULL),
(84, 'b8863534-dc73-4eef-959a-29938aebd457', 'Administrator', 'Support', 'message-circle', 'New customer support request', 'Rina Villanueva: Help checking upstairs Wi-Fi coverage', '/Admin/Support?status=All&ticketId=5', 'admin:support:5:open', '2026-10-01 22:26:34.812612', 0, NULL),
(85, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Jobs', 'tool', 'New job assigned to you', 'JOB-0015 for Rina Villanueva is scheduled for Oct 2, 2026 at 1:00 AM.', '/Technician/AssignedJobs?jobId=15', 'technician:request:15:pending', '2026-10-01 22:26:34.822295', 0, NULL),
(86, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Jobs', 'check-circle', 'Job marked completed', 'JOB-0014 for Maria Santos is scheduled for Sep 28, 2026 at 5:00 AM.', '/Technician/AssignedJobs?jobId=14', 'technician:request:14:completed', '2026-10-01 22:26:34.822295', 0, NULL),
(87, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Jobs', 'check-circle', 'Job marked completed', 'JOB-0013 for Maria Santos is scheduled for Oct 1, 2026 at 1:00 AM.', '/Technician/AssignedJobs?jobId=13', 'technician:request:13:completed', '2026-10-01 22:26:34.822295', 0, NULL),
(88, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Quotations', 'file-text', 'Quotation submitted for admin review', 'QT-0010 for JOB-0015 totals ₱4,850.00.', '/Technician/AssignedJobs?jobId=15', 'technician:quotation:10:pendingadmin', '2026-10-01 22:26:34.822295', 0, NULL),
(89, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Billing', 'check-circle', 'Invoice payment recorded', 'Payment of ₱8,350.00 has been recorded for INV-0009 on JOB-0013. Open the assigned job to review the next step.', '/Technician/AssignedJobs?jobId=13', 'technician:invoice:9:paid', '2026-10-01 22:26:34.822295', 0, NULL),
(90, 'c8145421-63dd-4a22-8769-b77d1fa8a9f8', 'Technician', 'Billing', 'check-circle', 'Invoice payment recorded', 'Payment of ₱4,250.00 has been recorded for INV-0011 on JOB-0014. Open the assigned job to review the next step.', '/Technician/AssignedJobs?jobId=14', 'technician:invoice:11:paid', '2026-10-01 22:26:34.822295', 0, NULL),
(91, 'e3608ff2-f7f6-4102-bc29-5e3dcb5dffdc', 'Customer', 'Jobs', 'wrench', 'Service request received', 'JOB-0016 is scheduled for Oct 3, 2026 at 2:00 AM. A technician will be assigned soon.', '/Customer/ServiceRequests?jobId=16', 'customer:request:16:pending:none', '2026-10-01 22:26:34.835541', 0, NULL),
(92, 'e3608ff2-f7f6-4102-bc29-5e3dcb5dffdc', 'Customer', 'Jobs', 'wrench', 'Service request received', 'JOB-0015 is scheduled for Oct 2, 2026 at 1:00 AM. Assigned technician: Aisha Lim.', '/Customer/ServiceRequests?jobId=15', 'customer:request:15:pending:none', '2026-10-01 22:26:34.835541', 0, NULL),
(93, 'e3608ff2-f7f6-4102-bc29-5e3dcb5dffdc', 'Customer', 'Quotations', 'file-text', 'Quotation is under admin review', 'QT-0010 for JOB-0015 totals ₱4,850.00.', '/Customer/Quotations', 'customer:quotation:10:pendingadmin', '2026-10-01 22:26:34.835541', 0, NULL),
(94, 'e3608ff2-f7f6-4102-bc29-5e3dcb5dffdc', 'Customer', 'Support', 'message-circle', 'Support request received', 'Your request “Help checking upstairs Wi-Fi coverage” is open.', '/Customer/Support?ticketId=5', 'customer:support:5:open:0', '2026-10-01 22:26:34.835541', 0, NULL);

--
-- Indexes for dumped tables
--

--
-- Indexes for table `ApplicationSettingAudits`
--
ALTER TABLE `ApplicationSettingAudits`
  ADD PRIMARY KEY (`AuditId`),
  ADD KEY `IX_ApplicationSettingAudits_ActorUserId` (`ActorUserId`);

--
-- Indexes for table `ApplicationSettings`
--
ALTER TABLE `ApplicationSettings`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_ApplicationSettings_UpdatedByUserId` (`UpdatedByUserId`);

--
-- Indexes for table `AspNetRoleClaims`
--
ALTER TABLE `AspNetRoleClaims`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_AspNetRoleClaims_RoleId` (`RoleId`);

--
-- Indexes for table `AspNetRoles`
--
ALTER TABLE `AspNetRoles`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `RoleNameIndex` (`NormalizedName`);

--
-- Indexes for table `AspNetUserClaims`
--
ALTER TABLE `AspNetUserClaims`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `IX_AspNetUserClaims_UserId` (`UserId`);

--
-- Indexes for table `AspNetUserLogins`
--
ALTER TABLE `AspNetUserLogins`
  ADD PRIMARY KEY (`LoginProvider`,`ProviderKey`),
  ADD KEY `IX_AspNetUserLogins_UserId` (`UserId`);

--
-- Indexes for table `AspNetUserRoles`
--
ALTER TABLE `AspNetUserRoles`
  ADD PRIMARY KEY (`UserId`,`RoleId`),
  ADD KEY `IX_AspNetUserRoles_RoleId` (`RoleId`);

--
-- Indexes for table `AspNetUsers`
--
ALTER TABLE `AspNetUsers`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `UserNameIndex` (`NormalizedUserName`),
  ADD KEY `EmailIndex` (`NormalizedEmail`);

--
-- Indexes for table `AspNetUserTokens`
--
ALTER TABLE `AspNetUserTokens`
  ADD PRIMARY KEY (`UserId`,`LoginProvider`,`Name`);

--
-- Indexes for table `Customers`
--
ALTER TABLE `Customers`
  ADD PRIMARY KEY (`CustomerID`),
  ADD KEY `IX_Customers_UserID` (`UserID`);

--
-- Indexes for table `Devices`
--
ALTER TABLE `Devices`
  ADD PRIMARY KEY (`DeviceID`),
  ADD KEY `IX_Devices_CustomerID` (`CustomerID`);

--
-- Indexes for table `GeneratedReports`
--
ALTER TABLE `GeneratedReports`
  ADD PRIMARY KEY (`GeneratedReportID`),
  ADD KEY `IX_GeneratedReports_GeneratedByUserID_GeneratedAtUtc` (`GeneratedByUserID`,`GeneratedAtUtc`);

--
-- Indexes for table `InventoryItems`
--
ALTER TABLE `InventoryItems`
  ADD PRIMARY KEY (`ItemID`);

--
-- Indexes for table `Invoices`
--
ALTER TABLE `Invoices`
  ADD PRIMARY KEY (`InvoiceID`),
  ADD KEY `IX_Invoices_RequestID` (`RequestID`);

--
-- Indexes for table `JobDeliverables`
--
ALTER TABLE `JobDeliverables`
  ADD PRIMARY KEY (`DeliverableID`),
  ADD KEY `IX_JobDeliverables_RequestID` (`RequestID`);

--
-- Indexes for table `JobInventoryUsages`
--
ALTER TABLE `JobInventoryUsages`
  ADD PRIMARY KEY (`UsageID`),
  ADD UNIQUE KEY `IX_JobInventoryUsages_RequestID_ItemID` (`RequestID`,`ItemID`),
  ADD KEY `IX_JobInventoryUsages_ItemID` (`ItemID`);

--
-- Indexes for table `ServiceMessages`
--
ALTER TABLE `ServiceMessages`
  ADD PRIMARY KEY (`MessageID`),
  ADD KEY `IX_ServiceMessages_RequestID` (`RequestID`),
  ADD KEY `IX_ServiceMessages_SenderID` (`SenderID`);

--
-- Indexes for table `ServiceRequests`
--
ALTER TABLE `ServiceRequests`
  ADD PRIMARY KEY (`RequestID`),
  ADD KEY `IX_ServiceRequests_CustomerID` (`CustomerID`),
  ADD KEY `IX_ServiceRequests_TechID` (`TechID`),
  ADD KEY `IX_ServiceRequests_TechID_ScheduledDate` (`TechID`,`ScheduledDate`);

--
-- Indexes for table `StockMovements`
--
ALTER TABLE `StockMovements`
  ADD PRIMARY KEY (`MovementID`),
  ADD KEY `IX_StockMovements_ItemID` (`ItemID`),
  ADD KEY `IX_StockMovements_RequestID` (`RequestID`);

--
-- Indexes for table `SupportTickets`
--
ALTER TABLE `SupportTickets`
  ADD PRIMARY KEY (`SupportTicketID`),
  ADD KEY `IX_SupportTickets_CustomerID` (`CustomerID`);

--
-- Indexes for table `Technicians`
--
ALTER TABLE `Technicians`
  ADD PRIMARY KEY (`TechID`),
  ADD KEY `IX_Technicians_UserID` (`UserID`);

--
-- Indexes for table `UserNotifications`
--
ALTER TABLE `UserNotifications`
  ADD PRIMARY KEY (`NotificationID`),
  ADD UNIQUE KEY `IX_UserNotifications_RecipientUserID_SourceKey` (`RecipientUserID`,`SourceKey`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `ApplicationSettingAudits`
--
ALTER TABLE `ApplicationSettingAudits`
  MODIFY `AuditId` bigint(20) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `ApplicationSettings`
--
ALTER TABLE `ApplicationSettings`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `AspNetRoleClaims`
--
ALTER TABLE `AspNetRoleClaims`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `AspNetUserClaims`
--
ALTER TABLE `AspNetUserClaims`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `Customers`
--
ALTER TABLE `Customers`
  MODIFY `CustomerID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `Devices`
--
ALTER TABLE `Devices`
  MODIFY `DeviceID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT for table `GeneratedReports`
--
ALTER TABLE `GeneratedReports`
  MODIFY `GeneratedReportID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `InventoryItems`
--
ALTER TABLE `InventoryItems`
  MODIFY `ItemID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `Invoices`
--
ALTER TABLE `Invoices`
  MODIFY `InvoiceID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=12;

--
-- AUTO_INCREMENT for table `JobDeliverables`
--
ALTER TABLE `JobDeliverables`
  MODIFY `DeliverableID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `JobInventoryUsages`
--
ALTER TABLE `JobInventoryUsages`
  MODIFY `UsageID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=17;

--
-- AUTO_INCREMENT for table `ServiceMessages`
--
ALTER TABLE `ServiceMessages`
  MODIFY `MessageID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT for table `ServiceRequests`
--
ALTER TABLE `ServiceRequests`
  MODIFY `RequestID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=17;

--
-- AUTO_INCREMENT for table `StockMovements`
--
ALTER TABLE `StockMovements`
  MODIFY `MovementID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=25;

--
-- AUTO_INCREMENT for table `SupportTickets`
--
ALTER TABLE `SupportTickets`
  MODIFY `SupportTicketID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `Technicians`
--
ALTER TABLE `Technicians`
  MODIFY `TechID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `UserNotifications`
--
ALTER TABLE `UserNotifications`
  MODIFY `NotificationID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=95;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `ApplicationSettingAudits`
--
ALTER TABLE `ApplicationSettingAudits`
  ADD CONSTRAINT `FK_ApplicationSettingAudits_AspNetUsers_ActorUserId` FOREIGN KEY (`ActorUserId`) REFERENCES `AspNetUsers` (`Id`);

--
-- Constraints for table `ApplicationSettings`
--
ALTER TABLE `ApplicationSettings`
  ADD CONSTRAINT `FK_ApplicationSettings_AspNetUsers_UpdatedByUserId` FOREIGN KEY (`UpdatedByUserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE SET NULL;

--
-- Constraints for table `AspNetRoleClaims`
--
ALTER TABLE `AspNetRoleClaims`
  ADD CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `AspNetUserClaims`
--
ALTER TABLE `AspNetUserClaims`
  ADD CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `AspNetUserLogins`
--
ALTER TABLE `AspNetUserLogins`
  ADD CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `AspNetUserRoles`
--
ALTER TABLE `AspNetUserRoles`
  ADD CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `AspNetUserTokens`
--
ALTER TABLE `AspNetUserTokens`
  ADD CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `Customers`
--
ALTER TABLE `Customers`
  ADD CONSTRAINT `FK_Customers_AspNetUsers_UserID` FOREIGN KEY (`UserID`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `Devices`
--
ALTER TABLE `Devices`
  ADD CONSTRAINT `FK_Devices_Customers_CustomerID` FOREIGN KEY (`CustomerID`) REFERENCES `Customers` (`CustomerID`) ON DELETE CASCADE;

--
-- Constraints for table `GeneratedReports`
--
ALTER TABLE `GeneratedReports`
  ADD CONSTRAINT `FK_GeneratedReports_AspNetUsers_GeneratedByUserID` FOREIGN KEY (`GeneratedByUserID`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `Invoices`
--
ALTER TABLE `Invoices`
  ADD CONSTRAINT `FK_Invoices_ServiceRequests_RequestID` FOREIGN KEY (`RequestID`) REFERENCES `ServiceRequests` (`RequestID`) ON DELETE CASCADE;

--
-- Constraints for table `JobDeliverables`
--
ALTER TABLE `JobDeliverables`
  ADD CONSTRAINT `FK_JobDeliverables_ServiceRequests_RequestID` FOREIGN KEY (`RequestID`) REFERENCES `ServiceRequests` (`RequestID`) ON DELETE CASCADE;

--
-- Constraints for table `JobInventoryUsages`
--
ALTER TABLE `JobInventoryUsages`
  ADD CONSTRAINT `FK_JobInventoryUsages_InventoryItems_ItemID` FOREIGN KEY (`ItemID`) REFERENCES `InventoryItems` (`ItemID`),
  ADD CONSTRAINT `FK_JobInventoryUsages_ServiceRequests_RequestID` FOREIGN KEY (`RequestID`) REFERENCES `ServiceRequests` (`RequestID`) ON DELETE CASCADE;

--
-- Constraints for table `ServiceMessages`
--
ALTER TABLE `ServiceMessages`
  ADD CONSTRAINT `FK_ServiceMessages_AspNetUsers_SenderID` FOREIGN KEY (`SenderID`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `FK_ServiceMessages_ServiceRequests_RequestID` FOREIGN KEY (`RequestID`) REFERENCES `ServiceRequests` (`RequestID`) ON DELETE CASCADE;

--
-- Constraints for table `ServiceRequests`
--
ALTER TABLE `ServiceRequests`
  ADD CONSTRAINT `FK_ServiceRequests_Customers_CustomerID` FOREIGN KEY (`CustomerID`) REFERENCES `Customers` (`CustomerID`),
  ADD CONSTRAINT `FK_ServiceRequests_Technicians_TechID` FOREIGN KEY (`TechID`) REFERENCES `Technicians` (`TechID`) ON DELETE SET NULL;

--
-- Constraints for table `StockMovements`
--
ALTER TABLE `StockMovements`
  ADD CONSTRAINT `FK_StockMovements_InventoryItems_ItemID` FOREIGN KEY (`ItemID`) REFERENCES `InventoryItems` (`ItemID`),
  ADD CONSTRAINT `FK_StockMovements_ServiceRequests_RequestID` FOREIGN KEY (`RequestID`) REFERENCES `ServiceRequests` (`RequestID`) ON DELETE SET NULL;

--
-- Constraints for table `SupportTickets`
--
ALTER TABLE `SupportTickets`
  ADD CONSTRAINT `FK_SupportTickets_Customers_CustomerID` FOREIGN KEY (`CustomerID`) REFERENCES `Customers` (`CustomerID`) ON DELETE CASCADE;

--
-- Constraints for table `Technicians`
--
ALTER TABLE `Technicians`
  ADD CONSTRAINT `FK_Technicians_AspNetUsers_UserID` FOREIGN KEY (`UserID`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `UserNotifications`
--
ALTER TABLE `UserNotifications`
  ADD CONSTRAINT `FK_UserNotifications_AspNetUsers_RecipientUserID` FOREIGN KEY (`RecipientUserID`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
