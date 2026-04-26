# Nalbur Management System

TR: WPF + MVVM mimarisi ile geliştirilmiş modern bir nalbur yönetim uygulaması.  
EN: A modern hardware store management application built with WPF + MVVM architecture.

---

## Özellikler / Features

### TR
- Dashboard ile kritik verileri görüntüleme
- Ürün ve stok yönetimi
- Müşteri yönetimi
- Nakit, kart ve taksitli satış
- Taksit takibi ve ödeme alma
- Satış geçmişi filtreleme
- Firma gider / ödeme takibi

### EN
- View critical data on the dashboard
- Product and stock management
- Customer management
- Cash, card, and installment sales
- Installment tracking and payment processing
- Sales history filtering
- Company expense / outgoing payment tracking

---

## Kullanılan Teknolojiler / Technologies

- .NET
- WPF
- MVVM
- CommunityToolkit.Mvvm
- Entity Framework Core
- SQL Server
- Material Design in XAML

---

## Proje Yapısı / Project Structure

```text
Nalbur.Domain
Nalbur.Infrastructure
Nalbur.Wpf
NalburSetupProject
TR
Nalbur.Domain → Entity ve interface katmanı
Nalbur.Infrastructure → Veri erişimi ve servis katmanı
Nalbur.Wpf → Arayüz ve ViewModel katmanı
NalburSetupProject → Kurulum paketi projesi
EN
Nalbur.Domain → Entity and interface layer
Nalbur.Infrastructure → Data access and service layer
Nalbur.Wpf → UI and ViewModel layer
NalburSetupProject → Installer package project
Kurulum / Setup
TR
script.sql dosyası ile veritabanını oluşturun.
appsettings.json içinde connection string ayarlayın.
Nalbur.Wpf projesini çalıştırın.
EN
Create the database using script.sql.
Configure the connection string in appsettings.json.
Run the Nalbur.Wpf project.

Publish / Setup
TR

Uygulama farklı bilgisayarlara kurulabilecek şekilde publish ve setup project desteğine sahiptir.

EN

The application supports publish and setup project workflows for installation on different computers.

Önerilen publish ayarları / Recommended publish settings:

Target: Folder
Configuration: Release
Deployment Mode: Self-contained
Target Runtime: win-x64

Not / Note
TR

Uygulamanın düzgün çalışması için hedef bilgisayarda veritabanı bağlantısının doğru yapılandırılması gerekir.

EN

For the application to work properly, the database connection must be correctly configured on the target machine.
TR

Bu proje özel / kurum içi kullanım amacıyla geliştirilmiştir.

EN

This project was developed for private / internal use.
