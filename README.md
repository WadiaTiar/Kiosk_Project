

## 📝 مشروع نظام الكشك الذكي | Smart Kiosk System
### **Arabic / العربية**

**الوصف العام:**
نظام إدارة كشك متكامل مبني بلغة **#C** ومعمارية الطبقات (**N-Tier Architecture**). يتميز النظام بمرونة فائقة في التعامل مع البيانات بفضل اعتماده على مفهوم **الأنطولوجيا الرقمية (Digital Ontology)**، مما يسمح بإضافة أنواع مختلفة من المنتجات بخصائص ديناميكية دون الحاجة لتعديل هيكل قاعدة البيانات.

**الميزات التقنية:**
* **معمارية منفصلة:** فصل تام بين طبقة واجهة المستخدم (UI)، منطق الأعمال (BAL)، والوصول للبيانات (DAL).
* **بيانات ديناميكية:** استخدام **JSON Metadata** لتخزين خصائص المنتجات المتغيرة (السعر، الوزن، اللون).
* **إدارة العلاقات:** نظام ربط متطور بين الأصناف والمنتجات (Parent-Child Relations) باستخدام SQL Server.
* **عمليات CRUD كاملة:** واجهة مستخدم بديهية لإضافة، عرض، تعديل، وحذف المنتجات مع نظام تأكيد آمن.

---

### **English / الإنجليزية**

**Overview:**
A robust **Smart Kiosk Management System** developed using **C# .NET** and following the **N-Tier Architecture** pattern. The core strength of this project lies in its **Ontology-based Data Modeling**, providing ultimate flexibility to handle diverse product types with dynamic attributes without altering the underlying database schema.

**Key Technical Features:**
* **Layered Architecture:** Strict separation of concerns between UI, Business Access Layer (BAL), and Data Access Layer (DAL).
* **Dynamic Metadata:** Leverages **JSON** to store polymorphic product attributes (Price, Weight, Color, etc.) efficiently.
* **Relational Integrity:** Advanced mapping between Categories and Items using a centralized `Relations` table in SQL Server.
* **Full CRUD Operations:** A streamlined user interface for Creating, Reading, Updating, and Deleting products with built-in data validation and transaction safety.

---

### **التقنيات المستخدمة / Tech Stack:**
* **Language:** C# (.NET WinForms)
* **Database:** SQL Server
* **Data Format:** JSON (Newtonsoft.Json)
* **Patterns:** Repository Pattern, N-Tier, Singleton.
