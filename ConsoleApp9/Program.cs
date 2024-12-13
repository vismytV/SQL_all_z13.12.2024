using ConsoleApp9;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

namespace ConsoleApp9
{
    public class Customer //таблиця клієнти
    {
        public int CustomerId { get; set; } // первинний ключ
        public string FirstName { get; set; } // Ім'я
        public string LastName { get; set; } // Прізвище
        public string? Email { get; set; } // Email

        public string login {  get; set; }
        public string password { get; set; }//пароль

        // Зв'язок з таблицею Orders (Один до багатьох)
        public ICollection<Order> Orders { get; set; }
    }

    public class Product //таблиця товари
    {
        public int ProductId { get; set; } // первинний ключ
        public string Name { get; set; } // Назва товару
        public decimal Price { get; set; } // Ціна
        public string? Description { get; set; } // Опис товару

        // Зв'язок з таблицею Orders (Багато до багатьох)
        public ICollection<OrderProduct> OrderProducts { get; set; }
    }

    public class Order //таблиця замовлення
    {
        public int OrderId { get; set; } // первинний ключ
        public DateTime OrderDate { get; set; } // Дата замовлення
        public int CustomerId { get; set; } // зовнішній ключ до Customers
        public decimal TotalAmount { get; set; } // Загальна сума замовлення

        // Зв'язок з таблицею Customer (Один до багатьох)
        public Customer Customer { get; set; }

        // Зв'язок з таблицею Product (Багато до багатьох)
        public ICollection<OrderProduct> OrderProducts { get; set; }
    }

    public class OrderProduct//проміжна таблиця для реалізації зв'язку між покупцум та тотоваром
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        public Order Order { get; set; }
        public Product Product { get; set; }
    }

    public class ShopDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=ShopDatabase.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // зв'язки між таблицями
            modelBuilder.Entity<OrderProduct>()
                .HasKey(op => new { op.OrderId, op.ProductId });//складений первинний ключ таблиці OrderProduct

            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Order)// Кожен запис у OrderProduct пов'язаний з одним Order
                .WithMany(o => o.OrderProducts)//Кожен Order може мати багато записів у OrderProduct
                .HasForeignKey(op => op.OrderId)//Вказує, що поле OrderId у таблиці OrderProduct є зовнішнім ключем, який посилається на Order.OrderId
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Product)//Кожен запис у OrderProduct пов'язаний з одним Product
                .WithMany(p => p.OrderProducts)//Кожен Product може бути присутнім у багатьох записах OrderProduct
                .HasForeignKey(op => op.ProductId)//Вказує, що поле ProductId у таблиці OrderProduct є зовнішнім ключем, який посилається на Product.ProductId
                .OnDelete(DeleteBehavior.Restrict);
            //видалення замовлення при видаленні користувача
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Product)
                .WithMany(p => p.OrderProducts)
                .HasForeignKey(op => op.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public void InitializeDatabase()
        {
            this.Database.EnsureCreated();
        }
    }


    public class Program
    {

        public static string first_name_vxid;
        public static int id_name_vxid;
        public static string last_name_vxid;
        public static string login_vxid;
        public static string email_vxid;

        public static string pokaz_menu;

        //стартове меню
        public static void menu_start()
        {
            pokaz_menu = "";
            Console.Clear();

            Console.WriteLine("1 -> вхід за повним ім'ям та паролем або за логіном та паролем ");
            Console.WriteLine("2 -> регістрація");
            Console.WriteLine("3 -> вхід як гість");
            Console.Write("\tЗробіть свій вибір:");
            char h;
            while (true)
            {
                ConsoleKeyInfo key= Console.ReadKey(true);
                if (key.KeyChar - '0' >= 1 && key.KeyChar - '0' <= 3) 
                {
                    h = key.KeyChar;
                    Console.Write(h);

                    break; 
                }
            }
            
            if (h == '1')
            {
                vxid();
            }
            else if (h == '2')
            {
                //registraciy();
                redag_info_customer(true);
            }
            else if (h == '3') 
            {
                first_name_vxid="";
            }
            G_menu(false);
        }
        public static void vxid()
        {
            Console.Clear();

            Console.WriteLine("=========== Вхід на сайт магазину продажу телефонів =========");
            while (true)
            {
                Console.Write("\nВведіть ім'я прізвище пароль (через пробіл), або логін пароль (через пробіл): ");
                string text = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(text))
                {
                    Console.WriteLine("\tНевірно введені дані.\n");
                    continue;
                }

                // Замінити багато пробілів на один
                text = Regex.Replace(text, @"\s+", " ");

                string[] vse_dan = text.Split(' ');
                if (vse_dan.Length < 2 || vse_dan.Length > 3)
                {
                    Console.WriteLine("\tНевірно введені дані.\n");
                    continue;
                }

                using (var context = new ShopDbContext())
                {
                    context.InitializeDatabase();

                    // Перевірка на ім'я, прізвище та пароль або логін і пароль
                    var customer = vse_dan.Length == 3
                        ? context.Customers.FirstOrDefault(c =>
                            c.FirstName == vse_dan[0] &&
                            c.LastName == vse_dan[1] &&
                            c.password == vse_dan[2])
                        : context.Customers.FirstOrDefault(c =>
                            c.login == vse_dan[0] &&
                            c.password == vse_dan[1]);

                    if (customer == null)
                    {
                        Console.WriteLine("\nКористувача з такими даними не знайдено.");
                        Thread.Sleep(2000);
                        first_name_vxid = "";
                        last_name_vxid = "";
                        login_vxid = "Гість";
                        email_vxid = "";

                        return;
                        
                    }

                    id_name_vxid = customer.CustomerId;

                    first_name_vxid = customer.FirstName;
                    last_name_vxid = customer.LastName;
                    login_vxid = customer.login;
                    email_vxid = customer.Email;
                    
                    /*if (customer.login == "admin")
                    {
                        login_vxid = "admin";
                        
                        return;
                    }*/

                    
                    return;
                }
            }
        }

        //виведення всих товарів
        public static void info_product()
        {
            pokaz_menu = "product";
            Console.Clear();
            hello();

            Console.WriteLine("====================== Всі товари ==============");
            Console.WriteLine($" ID |          Назва         |        ціна   ");
            Console.WriteLine("----|------------------------|-----------------");
            using (var context = new ShopDbContext())
            {
                context.InitializeDatabase();

                var products = context.Products.ToList();
                
                foreach (var p in products)
                {
                    Console.WriteLine($"{p.ProductId,3} | {p.Name,-23}| {p.Price,-21}");
                }
            }

            if (login_vxid == "admin")
            {
                Console.WriteLine("\n +  -> Додати новий товар:");
            }
            else if (login_vxid != "Гість")
            {
                Console.WriteLine("\n +  -> Придбати товар:");
            }

            G_menu(true);
        }

        //додавання нового товару
        public static void Add_product(string name,decimal price)
        {
            using (var context = new ShopDbContext())
            {
                context.InitializeDatabase();

                var product = new Product
                {
                    Name = name,
                    Price = price,
                    Description = "Товар супер"
                };
                context.Products.Add(product);
                context.SaveChanges();

                Console.WriteLine("\nТовар успішно додано");
            }
        }

        // Додавання нового покупця
        public static void Add_customer(string firstName, string lastName, string login1,string password1,string email)
        {
            using (var context = new ShopDbContext())
            {
                context.InitializeDatabase();

                var customer = new Customer
                {
                    FirstName = firstName,
                    LastName = lastName,
                    login = login1,
                    Email = email,
                    password= password1
                };

                context.Customers.Add(customer);
                context.SaveChanges();
                Console.WriteLine($"Користувач {customer.FirstName} {customer.LastName} доданий");
            }
        }

        // Виведення всіх покупців
        public static void info_customer()
        {
            pokaz_menu = "customer";
            Console.Clear();
            hello();

            Console.WriteLine("==================================== Всі покупці ===================================");
            Console.WriteLine($" ID |         Ім'я        |        Прізвище      |        login       | Email");
            Console.WriteLine("------------------------------------------------------------------------------------");
            using (var context = new ShopDbContext())
            {
                context.InitializeDatabase();

                var customers = context.Customers.ToList();

                foreach (var c in customers)
                {
                    if (c.login!="admin")
                    {
                        Console.WriteLine($" {c.CustomerId,-3}| {c.FirstName,-21}| {c.LastName,-21}| {c.login,-19}| {c.Email}");
                    }
                    
                }
            }

            G_menu(true);
        }

        //додаємо заказ покупцю
        public static void Add_order_to_customer(int customerId, List<int> productIds)
        {
            using (var context = new ShopDbContext())
            {
                context.InitializeDatabase();

                // Створюємо новий заказ
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    CustomerId = customerId,
                    TotalAmount = 0 
                };

                context.Orders.Add(order);
                context.SaveChanges(); //Зберігаємо замовлення(потрібно далі його Id)

                // Добавляем продукты в заказ через таблицу OrderProduct
                foreach (var productId in productIds)
                {
                    var product = context.Products.Find(productId);
                    if (product != null)
                    {
                        // Добавляем продукт в заказ
                        var orderProduct = new OrderProduct
                        {
                            OrderId = order.OrderId,
                            ProductId = product.ProductId
                        };
                        context.OrderProducts.Add(orderProduct);

                        // загальна сума
                        order.TotalAmount += product.Price;
                    }
                }

                context.SaveChanges();
                Console.WriteLine($"Замовлення від покупця з ID: {customerId} на {productIds.Count} товари виконано");
            }
        }

        //виведення всих замовлень
        public static void info_orders()
        {
            pokaz_menu = "orders";
            Console.Clear();
            hello();

            Console.WriteLine("==================================Всі замовлення ====================================");
            Console.WriteLine($" ID | Дата час замовлення |                 Покупець               | Загальна сума ");
            Console.WriteLine("------------------------------------------------------------------------------------");
            using (var context = new ShopDbContext())
            {
                context.InitializeDatabase();

                var orders = context.Orders.Include(o => o.Customer).Include(o => o.OrderProducts).ThenInclude(op => op.Product).ToList();

                // Фільтрація замовлень за CustomerId
                if (login_vxid != "admin")
                {
                    orders = context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                    .Where(o => o.CustomerId == id_name_vxid) // Фільтрація за ID користувача
                    .ToList();
                }
                
                foreach (var order in orders)
                {
                    string t1 = "Товари замовлення";
                    string t2 = "Ціна";

                    string name = order.Customer.FirstName + " " + order.Customer.LastName;
                    Console.WriteLine($" {order.OrderId,-2}| {order.OrderDate,-21}| {name,-39}| {order.TotalAmount}");
                    Console.WriteLine($"                             -------------------------------------");
                    Console.WriteLine($"                             {t1,-21}| {t2}");
                    Console.WriteLine($"                             -------------------------------------");
                    foreach (var orderProduct in order.OrderProducts)
                    {
                        Console.WriteLine($"                             {orderProduct.Product.Name,-21}| {orderProduct.Product.Price}");
                    }
                    Console.WriteLine("--------------------------------------------------------------------------------------");
                }
            }

            G_menu(true);

        }

        //введення тексту
        public static string vvod_str(string ymova,bool pysto=true)
        {
            Regex rez = new Regex(ymova);

            string dani = "";
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                // Вихід з введення (натиснення Enter)
                if (keyInfo.Key == ConsoleKey.Enter )
                {
                    if (dani == "" && pysto == false)
                    {
                        continue;
                    }
                    break;
                }

                // Видалення символу (натиснення Backspace)
                if (keyInfo.Key == ConsoleKey.Backspace && dani.Length > 0)
                {
                    dani = dani.Remove(dani.Length - 1); // Видаляємо останній символ
                    Console.Write("\b \b"); // Видаляємо символ із консолі
                    continue;
                }

                // Перевірка символу за регулярним виразом
                if (Regex.IsMatch(keyInfo.KeyChar.ToString(), ymova))
                {
                    dani += keyInfo.KeyChar; // Додаємо символ до введеного рядка
                    Console.Write(keyInfo.KeyChar); // Відображаємо символ
                }

            }
            return dani;
        }

        //перевірка дубліката товара
        private static bool perevirka_dublicate_product(string nazva)
        {
            bool rez = false;

            using (var context = new ShopDbContext())
            {
                context.InitializeDatabase();

                // Знайти товар
                var tovar = context.Products.FirstOrDefault(p => p.Name == nazva);

                if (tovar != null) {rez= true;}
            }

            return rez;
        }
            //перевірка дубліката логіна
            public static bool perevirka_dublicate_login(string login, int id=-1)
        {
            bool rez=true;

            using (var context = new ShopDbContext())
            {
                context.InitializeDatabase();

                // Знайти клієнта за його ID
                var redakt_customer = context.Customers.FirstOrDefault(c => c.CustomerId == id_name_vxid);

                //перевірка на повторення logina
                //var customer = context.Customers.FirstOrDefault(c => c.FirstName == firstName && c.LastName==lastName);
                var customer = context.Customers.FirstOrDefault(c => c.login == login);

                if (customer == null || customer.CustomerId == id_name_vxid)
                {
                    rez = false;
                }
                
                
            }

            return rez;
        }

        //редагування даних або регістрація
        public static void redag_info_customer(bool new_customer=false)
        {
            Console.Clear();
            bool pysto=false;
            string nov = "";
            string str1 = "";
            string str2 = "";
            string str3 = "";
            string str4 = "";

            if (new_customer == false)
            {
                Console.WriteLine($"{first_name_vxid} {last_name_vxid}   логин:{login_vxid}");
                Console.WriteLine("============Редагування даних==========");
                
                pysto = true;
                nov = "новий";
                str2 = $"(було '{last_name_vxid}',ENTER - залишити без змін)";
                str1 = $"(було '{first_name_vxid}', ENTER - залишити без змін)";
                str3 = $"(було '{login_vxid}',ENTER - залишити без змін)";
                str4 = $"(було '{email_vxid}',ENTER - залишити без змін)";
            }
            else
            {
                Console.WriteLine("=======================РЕГІСТРАЦІЯ====================");
            }

            Console.Write($"Введіть ім'я {str1}: ");
            string ymova = @"^[a-zA-zа-яА-ЯіІїЇї]+$";
            string firstName=vvod_str(ymova,pysto);
            if (firstName == "")
            {
                firstName = first_name_vxid;
                Console.Write(firstName);
            }

            Console.Write($"\nВведіть прізвище {str2}: ");
            string lastName = vvod_str(ymova,pysto);
            if (lastName == "")
            {
                lastName = last_name_vxid;
                Console.Write(lastName);
            }

            ymova = @"^[a-zA-z\d]+$";
            Console.Write($"\nВведіть login {str3} : ");
            string login = vvod_str(ymova);
            if (login == "")
            {
                login = login_vxid;
                Console.Write(login);
            }

            string email;
            while (true)
            {
                ymova = @"^[a-zA-z\d@\.]+$";
                Console.Write($"\nВведіть Email {str4}: ");
                email = vvod_str(ymova);
                if (email == "")
                {
                    email = email_vxid;
                    Console.Write(email);
                }

                ymova = @"^[a-zA-Z\d]+@+[a-zA-Z]+\.+[a-zA-Z]+$";
                Regex rez = new Regex(ymova);
                bool rez1 = rez.IsMatch(email);
                if (rez1 == true)
                {
                    break;
                }
                Console.WriteLine("\n\tПомилка при введенні email!");
            }
            
            ymova = @"^[a-zA-z\d]+$";
            Console.Write($"\nВведіть {nov} пароль: ");
            string pass1 = vvod_str(ymova,false);

            Console.Write($"\n\tПідтвердіть {nov} пароль: ");
            string pass2 = vvod_str(ymova, false);

            if (pass1 != pass2) 
            {
                Console.WriteLine("\nПароль не підтверджено. Дані не внесені");
                G_menu(true);
                return;
            }

            if (new_customer == false)//редагування
            {
                bool rez = perevirka_dublicate_login(login, id_name_vxid);

                if (rez == true)//есть дублікат
                {
                    Console.WriteLine($"\nТакий користувач уже є. Дані не внесені");
                    G_menu(true);
                    return;
                }

                using (var context = new ShopDbContext())
                {
                    context.InitializeDatabase();

                    // Знайти клієнта за його ID
                    var redakt_customer = context.Customers.FirstOrDefault(c => c.CustomerId == id_name_vxid);

                    // Оновити дані клієнта
                    redakt_customer.FirstName = firstName;
                    redakt_customer.LastName = lastName;
                    redakt_customer.Email = email;
                    redakt_customer.password = pass1;
                    redakt_customer.login = login;

                    // Зберегти зміни
                    context.SaveChanges();
                    Console.WriteLine("\nДані успішно оновлено!");

                }

                
            }
            else//регістрація
            {
                if (pass1 != pass2)
                {
                    Console.WriteLine("\nПароль не підтверджено. Дані не внесено");
                    Thread.Sleep(2000);
                    menu_start();
                    return;
                }

                bool rez = perevirka_dublicate_login(login);

                if (rez == true)//есть дублікат
                {
                    Console.WriteLine($"\nТакий користувач уже є. Дані не внесено");
                    Thread.Sleep(2000);
                    menu_start();
                    return;
                }
                Console.WriteLine();
                Add_customer(firstName, lastName, login, pass1, email);

            }

            first_name_vxid = firstName;
            last_name_vxid = lastName;
            email_vxid = email;
            login_vxid = login;

            G_menu(true);
            return;

        }
        //головне меню

        //привітання 
        public static void hello()
        {
            if (login_vxid != "" && login_vxid != null)
            {
                Console.WriteLine($"\tВітаю {first_name_vxid} {last_name_vxid} login:{login_vxid}  email:{email_vxid}");
            }
            else
            {
                login_vxid = "Гість";
                Console.WriteLine("Вітаю Гість");
            }
        }

        //введення нового товару та ціни
        public static void plus_product()
        {
            Console.Write("Введіть назву товару: ");
            string ymova = @"^[a-zA-zа-яА-ЯіІїЇї\d. ]+$";
            string name_product = vvod_str(ymova, false);
            decimal cina;
            while (true)
            {
                Console.Write("\nВведіть ціну товару: ");
                ymova = @"^[\d.]+$";
                string price_product = vvod_str(ymova, false);

                try
                {
                    cina = decimal.Parse(price_product);
                    break;
                }
                catch
                {
                    Console.WriteLine("\tПомилка при введенні ціни!\n");
                }
            }

            bool rez = perevirka_dublicate_product(name_product);
            if (rez == true)
            {
                Console.WriteLine("\nТакий продукт уже існує");
            }
            else
            {
                Add_product(name_product, cina);
            }

            Thread.Sleep(2000);

            info_product();

        }
        public static void G_menu(bool start=false)
        {
            if (start == true)
            {
                Console.WriteLine("\nНатисніть ENTER -> на головне меню");
                while (true)
                {
                    ConsoleKeyInfo key1 = Console.ReadKey(true);
                    if ((int)key1.KeyChar == 13) { break;}
                    
                    if (login_vxid == "admin")
                    {
                        if (key1.KeyChar=='+' && pokaz_menu == "product")
                        {
                            plus_product();
                        }
                    }
                    else if (login_vxid != "Гість")
                    {
                        if (key1.KeyChar == '+' && pokaz_menu == "product")
                        {
                            Console.WriteLine("Функція на стадії розробки");
                            Thread.Sleep(2000);
                            info_product();
                        }
                    }
                }
            }

            pokaz_menu = "";
            Console.Clear();

            hello();
            
            Console.WriteLine($"========================== ГОЛОВНЕ МЕНЮ =====================");
            if (login_vxid == "admin")
            {
                Console.WriteLine("    1    -> Показати всих користувачів (покупців)");
                Console.WriteLine("    2    -> Показати всі чеки");
                Console.WriteLine("    3    -> Видалити користувача та всі дані з ним пов'язані");
            }
            else if (login_vxid != "Гість")
            {
                Console.WriteLine("    2    -> Показати всі свої чеки");
                Console.WriteLine("    3    -> Змінити свої дані");
            }

            Console.WriteLine("    4    -> Показати всі товари");
            
            Console.WriteLine(" інакше  -> вихід");

            ConsoleKeyInfo key = Console.ReadKey(true);
            switch (key.KeyChar)
            {
                case '1':
                    if (login_vxid == "admin")
                    {
                        info_customer();
                    }
                    
                    break;
                case '2':
                    if (login_vxid != "Гість")
                    {
                        info_orders();
                    }

                    break;
                case '3':
                    if (login_vxid == "admin")
                    {
                        DeleteCustomer();//видалення
                        break;
                    }
                    redag_info_customer();//редагування
                    break;
                case '4':
                    info_product();

                    break;

                default:
                    menu_start();
                    break;
            }

        }
        //видалення користувача та пов'язаних з ним даних
        public static void DeleteCustomer()
        {
            pokaz_menu = "";
            Console.WriteLine("\nДЛЯ ВИДАЛЕННЯ");
            Console.WriteLine("Введіть ID користувача або його login:");
            string input = Console.ReadLine();

            using (var context = new ShopDbContext())
            {
                context.InitializeDatabase();

                Customer customer = null;

                bool nom = true;
                if (int.TryParse(input, out int customerId))
                {
                    // Пошук користувача за ID
                    customer = context.Customers
                    .Include(c => c.Orders) // Завантажити пов'язані замовлення
                    .ThenInclude(o => o.OrderProducts) // Завантажити товари у замовленнях
                    .FirstOrDefault(c => c.CustomerId == customerId);
                }
                else
                {
                    nom = false;
                    // Пошук користувача за loginom
                    string login = input;
                        
                    customer = context.Customers
                    .Include(c => c.Orders)
                    .ThenInclude(o => o.OrderProducts)
                    .FirstOrDefault(c => c.login == login);
                }

                if (customer != null )
                {
                    if (customer.login == "admin")
                    {
                        Console.WriteLine("Неможна видалити адміна");
                        G_menu(true);
                        return;
                    }

                    // Видалення пов'язаних замовлень і товарів
                    foreach (var order in customer.Orders)
                    {
                        context.OrderProducts.RemoveRange(order.OrderProducts);
                        context.Orders.Remove(order);
                    }

                    // Видалення користувача
                    context.Customers.Remove(customer);
                    context.SaveChanges();

                    Console.WriteLine($"Користувач \"{customer.FirstName} {customer.LastName}\" і всі його дані були успішно видалені.");
                }
                else
                {
                    if (nom == true)
                    {
                        Console.WriteLine($"Користувача з ID:{input} не знайдено.");
                    }
                    else
                    {
                        Console.WriteLine($"Користувача '{input}' не знайдено.");
                    }
                    
                }
            }

            pokaz_menu = "";
            G_menu(true);
        }


        public static void Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.InputEncoding = Encoding.GetEncoding("windows-1251");
            Console.OutputEncoding = Encoding.UTF8;


            //Add_product();
            //info_product();

            /*string firstName = "q";
            string lastName = "q2";
            string login = "1";
            string email = "jshgdfsjdf";
            string pass = "1";
            Add_customer(firstName, lastName, login, pass,email);
            info_customer();
*/

            //List<int> productIds = new List<int> { 3,4 }; //замовляє товар з з ID=3 та  ID=4
            //Add_order_to_customer(2, productIds);//замовлення робить покупець з ID=2

            // Виведення всіх замовлень
            //info_orders();

            //ГОЛОВНЕ МЕНЮ
            //G_menu();

            menu_start();



        }
    }

}
