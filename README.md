# 🏥 ClinicFlow — ASP.NET Core Clinic Management System  

Hey everyone! 👋  
Welcome to **ClinicFlow**, a robust and secure **Clinic Management System API** built from scratch using **ASP.NET Core Web API**.  

This project is designed with a **clean and organized structure**, **role-based access control**, and **enterprise-level security** — making it ideal for managing modern clinic operations efficiently.  

---

## 🚀 Key Highlights

### 🔐 Authentication & Authorization
- **JWT Authentication** for secure token-based login.  
- **Role-Based Access Control** for:
  - 👑 **Admin** – Full control over doctors, patients, departments, and appointments.  
  - 🩺 **Doctor** – Can access only their own patients and records.  
  - 👤 **Patient** – Can view only their personal and medical information.

### 🧩 Secure Data Handling
- **ASP.NET Identity** for user and role management.  
- **DTOs** for safe and validated data transfer.  
- **Entity Framework Core** for database management.  
- **Middleware** to track API response times and performance.  

### 🧠 Data Integrity Rules
- Admins **cannot delete** entities that have dependencies —  
  e.g., a doctor with existing appointments or a patient with medical records — ensuring full data integrity and safety.

### 💊 Core System Modules
- Doctors 👨‍⚕️  
- Patients 👩‍⚕️  
- Departments 🏢  
- Appointments 🗓️  
- Prescriptions 💊  
- Medical Records 📋  
- Users 🔐  

### 🧾 Documentation & Testing
- Built-in **Swagger UI** for live API documentation.  
- **Postman Collection** available for testing endpoints easily.  

---
## 🧱 Project Structure
  ┣ 📂 Controllers
  ┣ 📂 DTOs
  ┣ 📂 Models
  ┣ 📂 Data
  ┣ 📂 Middleware
  ┣ 📂 Services
  ┣ 📜 Program.cs
  ┣ 📜 appsettings.json

---
## 🧠 Technologies Used
- ASP.NET Core 8 Web API  
- Entity Framework Core  
- ASP.NET Identity  
- JWT Authentication  
- AutoMapper  
- LINQ & IQueryable  
- Middleware  

---

⭐ **If you like this project, don’t forget to give it a star!**  
📫 *Developed by [Kerolos Adel]*  
💼 Connect with me on [LinkedIn](https://www.linkedin.com/in/kerolos-adel-190948375/)

















Organized using a **Clean Structure** approach for clarity and scalability:  

