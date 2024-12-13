using ConsoleApp9;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp9
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Role { get; set; } // Адмін або Користувач
        public string Password { get; set; }
        
    }
    public class FuelHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }  // ID користувача
        public int GazStationId { get; set; } // ID АЗС
        public int GazStationPointId { get; set; } // ID колонки

        public int LocationID { get; set; }//ID міста
        public int Amount { get; set; }  // Кількість пального
        public DateTime Date { get; set; }  // Дата заправки

        // Зв'язок з іншими таблицями
        public virtual GazStation GazStation { get; set; }
        public virtual GazStationPoint GazStationPoint { get; set; }

        public virtual Location Location { get; set; }
    }
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; } // Назва міста
    }
    public class GazStation
    {
        public int Id { get; set; }
        public string Name { get; set; } // Назва АЗС
        public Location Location { get; set; } // Місцезнаходження АЗС

        public string Nomer {  get; set; }//номер АЗС
        public List<GazStationPoint> GazStationPoints { get; set; } // Список колонок
    }
    public class GazStationPoint
    {
        public int Id { get; set; }
        public string FuelType { get; set; } // Тип пального
        public int Capacity { get; set; } // Ємність колонки
        public int CurrentAmount { get; set; } // Поточний рівень пального
    }
    public class GasStationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<GazStation> GazStations { get; set; }
        public DbSet<GazStationPoint> GazStationPoints { get; set; }

        public DbSet<FuelHistory> FuelHistories { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=gas_station.db");
        }

        
        public static void InitializeDatabase(GasStationContext db)
        {
            // Перевірка наявності користувачів і додавання
            if (!db.Users.Any())
            {
                db.Users.Add(new User { Login = "Admin", Role = "admin", Password = "admin" });
                db.Users.Add(new User { Login = "User1", Role = "user", Password = "user1" });
                db.SaveChanges();
            }

            // Перевірка наявності локацій
            if (!db.Locations.Any())
            {
                db.Locations.Add(new Location { Name = "Київ" });
                db.Locations.Add(new Location { Name = "Суми" });
                db.SaveChanges();
            }

            // Перевірка наявності заправних станцій
            if (!db.GazStations.Any())
            {
                var kyivLocation = db.Locations.First(l => l.Name == "Київ");
                var sumyLocation = db.Locations.First(l => l.Name == "Суми");

                db.GazStations.Add(new GazStation
                {
                    Name = "OKKO",
                    Location = kyivLocation,
                    Nomer = "№1",
                    GazStationPoints = new List<GazStationPoint>
        {
            new GazStationPoint { FuelType = "A-95", Capacity = 5000, CurrentAmount = 5000 },
            new GazStationPoint { FuelType = "A-92", Capacity = 4000, CurrentAmount = 4000 }
        }
                });

                db.GazStations.Add(new GazStation
                {
                    Name = "RRK",
                    Location = sumyLocation,
                    Nomer = "№1",
                    GazStationPoints = new List<GazStationPoint>
        {
            new GazStationPoint { FuelType = "A-95", Capacity = 6000, CurrentAmount = 6000 },
            new GazStationPoint { FuelType = "A-92", Capacity = 5000, CurrentAmount = 5000 }
        }
                    
                });

                db.SaveChanges();  // Оновлення бази після додавання станцій і колонок

                
            }

        }
    }
    public class Program
    {
        //введення тексту
        public static string vvod_str(string ymova, bool pysto = false)
        {
            Regex rez = new Regex(ymova);

            string dani = "";
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                // Вихід з введення (натиснення Enter)
                if (keyInfo.Key == ConsoleKey.Enter)
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
        // Перевірка логіна та пароля користувача в базі(вхід)
        public static int prov_users(string login, string pasw)
        {
            int rez = -1;

            using (var context = new GasStationContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Login == login && u.Password == pasw);
                if (user != null)
                {
                    rez = 1;
                    if (user.Role == "admin")
                    {
                        rez = 0;
                    }
                }
                
            }

            return rez;
        }
        //привітання
        public static void hello(string login)
        {
            Console.Clear();
            Console.WriteLine($"Вітаємо {login}");
        }

        public static void pokaz_vsi_mista(string login)
        {
            using (var context = new GasStationContext())
            {
                var location = context.Locations.ToList();
                foreach (var p in location)
                {
                    string a = "ID:"+p.Id.ToString()+"; ";
                    if (login != "Admin")
                    {
                        a = "";
                    }

                    Console.WriteLine($"\t{a}{p.Name}");
                }

            }
            
        }

        //зміна даних колонки
        public static void zmina_dan_kolonka(string misto, int id_AZS, int id_kolonka,string tup_paluva="", int new_emnist=-1,int new_kol_paluva=-1)
        {
            using (var context = new GasStationContext())
            {
                // Находимо локацію (місто) по назві
                var location = context.Locations.FirstOrDefault(u => u.Name == misto);

                // Знаходимо АЗС з певним ID в даному місті
                var station = context.GazStations
                    .Include(g => g.GazStationPoints)  // Додаємо завантаження колонок
                    .FirstOrDefault(g => g.Location.Id == location.Id && g.Id == id_AZS);

                // знаходимо колонку з певним ID
                var point = station.GazStationPoints.FirstOrDefault(p => p.Id == id_kolonka);


                //вносимо зміни
                if (new_emnist != -1)
                {
                    point.Capacity = new_emnist;
                }
                
                if (new_kol_paluva != -1)
                {
                    point.CurrentAmount = new_kol_paluva;
                }
                
                if (tup_paluva != "")
                {
                    point.FuelType = tup_paluva;
                }
                

                context.SaveChanges();
                

            }
                
           
        }

        //перевірка ID колонки
        public static int prov_id_kolonku(string misto,int id_AZS, int id_kolonka)
        {
            int rez = -1;
            using (var context = new GasStationContext())
            {
                // Находимо локацію (місто) по назві
                var location = context.Locations.FirstOrDefault(u => u.Name == misto);

                // Знаходимо АЗС з певним ID в даному місті
                var station = context.GazStations
                    .Include(g => g.GazStationPoints)  // Додаємо завантаження колонок
                    .FirstOrDefault(g => g.Location.Id == location.Id && g.Id == id_AZS);

                // Перевірка на наявність колонки з певним ID
                var point = station.GazStationPoints.FirstOrDefault(p => p.Id == id_kolonka);

                if (point != null)
                {
                    rez = point.CurrentAmount;
                }
                
            }


            return rez;
        }

        //перевірка ID АЗС
        public static bool prov_id_azs(int id, string misto)
        {
            bool rez = false;

            using (var context = new GasStationContext())
            {
                // Находимо локацію (місто) по назві
                var location = context.Locations.FirstOrDefault(u => u.Name == misto);

                var stations = context.GazStations.FirstOrDefault(g => g.Location.Id == location.Id && g.Location.Id == id);
                if (stations!=null)
                {
                    rez=true;
                }
            }
            
            return rez;
        }
        public static bool pokaz_kolonku_mista(string misto, string role)
        {
            bool rez = false;
            using (var context = new GasStationContext())
            {
                // Находим локацию (город) по названию
                var location = context.Locations.FirstOrDefault(u => u.Name == misto);
                
                // Находимо всі AZS станції в цьому місті, включаючи колонки (GazStationPoints)
                var stations = context.GazStations
                    .Include(g => g.GazStationPoints)  // Додаємо завантаження колонок
                    .Where(g => g.Location.Id == location.Id)
                    .ToList();

                if (stations.Any())
                {
                    Console.WriteLine($"\n---інформація про всі АЗС міста {misto}:");
                    foreach (var station in stations)
                    {
                        Console.WriteLine($"\tID: {station.Id} АЗС: {station.Name} {station.Nomer}");

                        // Проверка на наличие колонок и вывод информации по ним
                        if (station.GazStationPoints != null && station.GazStationPoints.Any())
                        {
                            foreach (var point in station.GazStationPoints)
                            {
                                Console.WriteLine($"ID:{point.Id}  Колонка : {point.FuelType}," +
                                    $" Поточний рівень: {point.CurrentAmount} л");
                            }
                            rez = true;
                            //return true;
                        }
                        else
                        {
                            Console.WriteLine("  Немає колонок на цій АЗС.");
                            //return false;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"\n\tВ місті {misto} немає заправок.");
                }
            }

            return rez;

        }
        public static void zapravutu_avto(string login, string pasw, bool pokaz = true)
        {
            hello(login);  // Вывод приветствия
            Console.WriteLine("\t0 -> до попереднього меню");

            if (pokaz)
            {
                Console.WriteLine("\t1 -> показати всі міста");
            }
            else
            {
                Console.WriteLine("\n----Всі міста:");
                pokaz_vsi_mista(login);  // Показать все города
            }

            Console.Write("\nВведіть назву міста: ");

            string ymova = @"^[а-яА-ЯіІїЇ01]+$";
            string misto = vvod_str(ymova);  // Ввод города с проверкой на допустимые символы

            if (misto == "0")
            {
                menu_user(login, pasw);  // Возврат в предыдущее меню
            }
            else if (misto == "1")
            {
                zapravutu_avto(login, pasw, false);  // Повторный вызов для показа всех городов
                return;
            }

            int id_misto;
            using (var context = new GasStationContext())
            {
                // Находим локацию (город) по названию
                var location = context.Locations.FirstOrDefault(u => u.Name == misto);
                if (location == null)
                {
                    Console.WriteLine($"\n\t{misto} -> Такого міста не знайдено.");
                    Thread.Sleep(3000);
                    zapravutu_avto(login, pasw, false);  // Если город не найден, перезапускаем метод
                    return;
                }

                id_misto=location.Id;
                bool rez=pokaz_kolonku_mista(misto,login);
                if (rez == false)
                {
                    return;//немає
                }
            }

            int id_AZS;
            ymova = @"^[\d]+$";
            while (true)
            {
                Console.Write("\nВведіть ID заправки: ");
                id_AZS = int.Parse(vvod_str(ymova));
                bool rez = prov_id_azs(id_AZS,misto);

                if (rez== false)
                {
                    Console.WriteLine($"\n\tВ місті {misto} АЗС з таким ID не знайдено!");
                    continue;
                }
                break;
            }

            int kol_paluva;
            int id_kolonka;
            while (true)
            {
                Console.Write("\nВведіть ID колонки: ");
                id_kolonka = int.Parse(vvod_str(ymova));

                kol_paluva = prov_id_kolonku(misto, id_AZS, id_kolonka);

                if (kol_paluva == -1)
                {
                    Console.WriteLine($"\n\tВ місті {misto} на АЗС з ID {id_AZS} колонки з ID {id_kolonka} не знайдено!");
                    continue;
                }
                break;
            }

            if (kol_paluva == 0)
            {
                Console.WriteLine("Нажаль дана колонка порожня. Виберіть іншу");
                return;
            }
            
            Console.Write("\nВведіть скільки ви хочете залити палива: ");
            int kol_zalut= int.Parse(vvod_str(ymova));

            if (kol_zalut== 0)
            {
                return;
            }
            
            if (kol_zalut > kol_paluva)
            {
                Console.WriteLine($"Нажаль на даній колонці лише {kol_paluva}л. Виберіть іншу");
                return;
            }

            kol_paluva -= kol_zalut;
            
            zmina_dan_kolonka(misto, id_AZS, id_kolonka, "", -1, (int)kol_paluva);

            // Додаємо запис до історії заправок
            using (var context = new GasStationContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Login == login);
                var fuelHistory = new FuelHistory
                {
                    UserId = user.Id,
                    GazStationId = id_AZS,
                    GazStationPointId = id_kolonka,
                    LocationID=id_misto,
                    Amount = kol_zalut,
                    Date = DateTime.Now // Тепер записуємо дату і час заправки
                };

                context.FuelHistories.Add(fuelHistory);
                context.SaveChanges();
            }
        }


        public static void ViewFuelHistory(string login)
        {
            hello(login);

            using (var context = new GasStationContext())
            {
                // Знаходимо користувача по логіну
                var user = context.Users.FirstOrDefault(u => u.Login == login);

                // Отримуємо історію заправок для цього користувача
                var fuelHistory = context.FuelHistories
                    .Where(f => f.UserId == user.Id)
                    .Include(f => f.Location) // Включаємо місто
                    .Include(f => f.GazStation) // Включаємо заправку
                    .Include(f => f.GazStationPoint) // Включаємо колонку
                    .OrderByDescending(f => f.Date) // Сортуємо по даті, остання заправка на початку
                    .ToList();

                if (fuelHistory.Count == 0)
                {
                    Console.WriteLine("У вас немає історії заправок.");
                    return;
                }

                // Виводимо історію заправок
                Console.WriteLine("--------------------------------Історія заправок---------------------------|");
                Console.WriteLine("     Дата та час    |          Місто          |     АЗС   |паливо|кількість|");
                Console.WriteLine("--------------------|-------------------------|-----------|------|---------|");
                foreach (var record in fuelHistory)
                {
                    Console.WriteLine($" {record.Date,-18}| {record.Location.Name,-24}| {record.GazStation.Name,-10}|" +
                        $" {record.GazStationPoint.FuelType,-5}| {record.Amount,-8}");
                }
            }
        }

        public static void new_password(string login, string pasw)
        {
            Console.WriteLine("\nВведіть старий пароль");
            string ymova = @"^[a-zA-z\d]+$";
            string star_pasw = vvod_str(ymova);

            if (star_pasw != pasw)
            {
                Console.WriteLine("Невірний пароль");
                Thread.Sleep(3000);
                menu_user(login, pasw);
            }

            Console.Write("\n\tВведіть новий пароль: ");
            star_pasw = vvod_str(ymova);

            Console.Write("\n\tПідтвердіть новий пароль: ");
            string new_pasw = vvod_str(ymova);

            if (new_pasw != star_pasw)
            {
                Console.WriteLine("Паролі не співпадають. Спробуйте ще раз.");
                Thread.Sleep(3000);
                menu_user(login, pasw);
                return;
            }

            using (var context = new GasStationContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Login == login);

                user.Password = new_pasw;
                context.SaveChanges();
            }

        }

        public static void menu_user(string login, string pasw, bool povtor=false)
        {
            if (povtor == false)
            {
                hello(login);
            }
            else{
                Console.WriteLine();
            }

            Console.WriteLine("====================== МЕНЮ ========================");
            Console.WriteLine(" 1 -> Заправити авто");
            Console.WriteLine(" 2 -> Переглянути історію");
            Console.WriteLine(" 3 -> Змінити пароль");
            Console.WriteLine(" 0 -> Вихід");
            Console.Write("\n\tЗробіть свій вибір: ");

            char h;
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                
                h=keyInfo.KeyChar;
                if (h=='0' || h == '1' || h=='2' || h=='3')
                {
                    Console.Write(h);
                    break;
                }
            }

            switch (h)
            {
                case '0':
                    StartMenu();
                    break;
                case '1':
                    zapravutu_avto(login, pasw);
                    menu_user(login, pasw, true);
                    break;
                case '2':
                    ViewFuelHistory(login);
                    menu_user(login, pasw,true);
                    break;
                case '3':
                    new_password(login,pasw);
                    menu_user(login, pasw);
                    break;

            }


        }

        public static void all_users(string login)
        {
            hello(login);

            using (var context = new GasStationContext())
            {
                var users = context.Users.ToList();

                Console.WriteLine("------- Всі користувачі ---------");
                Console.WriteLine(" №  | ID |         Login          ");
                Console.WriteLine("----|----|------------------------");

                int kol = 0;
                foreach (var user in users)
                {
                    if (user.Role != "admin")
                    {
                        Console.WriteLine($" {++kol,-3}| {user.Id,-3}| {user.Login}");
                    }
                    
                }
                Console.WriteLine("\n============= МЕНЮ =============");
                Console.WriteLine(" +  -> Додати користувача");
                Console.WriteLine("-ID -> Видалити користувача");
            }

        }

        public static void edit_misto(int id,string login,string pasw)
        {
            using (var context = new GasStationContext())
            {
                var location = context.Locations.FirstOrDefault(u => u.Id == id);
                
                Console.Write($"\n\tВведіть нову назву, було '{location.Name}': ");
                string ymova = @"^[а-яА-ЯіІїЇ]+$";
                string misto = vvod_str(ymova);

                var location1 = context.Locations.FirstOrDefault(u => u.Name == misto);
                if (location1 != null)
                {
                    Console.WriteLine($"Міcто {misto} уже є в базі");
                }
                else
                {
                    location.Name = misto;
                    context.SaveChanges();

                    Console.WriteLine($"Назву міста успішно змінено");
                }
                
                
            }

            Thread.Sleep(3000);
            menu_admin(login, pasw, false, ' ');
            return;

        }

        public static void mew_misto(string login, string pasw)
        {
            using (var context = new GasStationContext())
            {
                
                Console.Write("\nВведіть назву нового міста:");
                string ymova = @"^[а-яА-ЯіІїЇ]+$";
                string misto = vvod_str(ymova);

                var location = context.Locations.FirstOrDefault(u => u.Name==misto);
                if (location != null)
                {
                    Console.WriteLine($"\n\tМіcто {misto} уже є в базі");
                }
                else
                {
                    var newLocation = new Location
                    {
                        Name = misto
                    };

                    context.Locations.Add(newLocation);  
                    context.SaveChanges();  

                    Console.WriteLine($"\nМісто {misto} успішно додано до бази.");
                }
                

            }


            Thread.Sleep(3000);
            menu_admin(login, pasw, false, ' ');
            return;


        }

        public static void del_misto(int id, string login, string pasw)
        {
            using (var context = new GasStationContext())
            {
                // Шукаємо місто за переданим id
                var location = context.Locations.FirstOrDefault(l => l.Id == id);

                // Видалення історії заправок, що пов'язані з цим містом
                var fuelHistories = context.FuelHistories.Where(fh => fh.LocationID == location.Id).ToList();
                if (fuelHistories.Any())
                {
                    context.FuelHistories.RemoveRange(fuelHistories); // Видалення історії заправок
                }

                // Видалення заправних станцій та пунктів заправок, що знаходяться в цьому місті
                var gazStations = context.GazStations.Where(gs => gs.Location.Id == location.Id).ToList();

                if (gazStations.Any())
                {
                    // Видаляємо пункти заправки для кожної заправної станції
                    foreach (var station in gazStations)
                    {
                        context.GazStationPoints.RemoveRange(station.GazStationPoints);
                    }

                    // Видаляємо заправні станції
                    context.GazStations.RemoveRange(gazStations);
                }

                
                // Видаляємо саме місто
                context.Locations.Remove(location);
                context.SaveChanges();  // Зберігаємо зміни в базі

                // Повідомлення про успішне видалення
                Console.WriteLine($"\nМісто {location.Name} та всі зв'язані з ним дані успішно видалені з бази.");
                Thread.Sleep(3000);
                menu_admin(login, pasw, false, ' '); 
            }
        }


        public static void edit_azc(string misto,int id,string login,string pasw)
        {
            using (var context = new GasStationContext())
            {
                // Находим локацию (город) по названию
                var location = context.Locations.FirstOrDefault(u => u.Name == misto);

                // Находимо AZS по ИД
                var stations = context.GazStations
                    .Include(g => g.GazStationPoints)  // Додаємо завантаження колонок
                    .FirstOrDefault(g => g.Location.Id == location.Id && g.Id==id);

                if (stations == null)
                {
                    Console.WriteLine($"\nAЗС з ID {id} в місті {misto} не знайдено");
                    Thread.Sleep(3000);
                }
                else
                {
                    string ymova = @"^[a-zA-Zа-яА-ЯіІЇї\d]";
                    Console.Write($"\nВведіть назву АЗС було {stations.Name}: ");
                    string name_azs = vvod_str(ymova);

                    ymova = @"^[\d]";
                    Console.Write($"\nВведіть номер АЗС {name_azs} було {stations.Nomer}: ");
                    string nomer = "№" + vvod_str(ymova);

                    var stations1 = context.GazStations
                        .Include(g => g.GazStationPoints)  // Додаємо завантаження колонок
                        .FirstOrDefault(g => g.Location.Id == location.Id && g.Name == name_azs && g.Nomer == nomer);

                    if (stations1 != null)
                    {
                        Console.WriteLine($"\nAЗС {name_azs} в місті {misto} вже є");

                    }
                    else
                    {
                        Console.WriteLine("\nДані успішно змінено");
                        stations.Name = name_azs;
                        stations.Nomer = nomer;
                        context.SaveChanges();
                    }
                }

                
                
            }

            Thread.Sleep(3000);

            hello(login);

            bool rez2 = pokaz_kolonku_mista(misto, login);

            menu_admin_AZS(login, pasw, misto, rez2);
            
        }
        public static void menu_admin_AZS(string login, string pasw,string misto,bool azs=true)
        {
            Console.WriteLine("\n================== МЕНЮ =====================");
            Console.WriteLine("  +  -> Додати АЗС");
            if (azs == true)
            {
                Console.WriteLine(" +ID -> Редагувати назву АЗС");
                Console.WriteLine(" -ID -> Видалити АЗС та дані з нею пов'язані");
                Console.WriteLine("  ID -> Редагувати колонки АЗС");
            }
            
            Console.WriteLine("  0  -> До попереднього меню");
            Console.Write("\n\tЗробіть свій вибір: ");

            string ymova;
            int rez;
            ymova = @"^[\+\-\d]";
            string rez1 = vvod_str(ymova);
            if (rez1 == "0")
            {
                menu_admin(login, pasw,true,'1');
                return;
            }
            else if (rez1 == "+")
            {
                /*mew_misto(login, pasw);
                return;*/
            }

            try
            {
                rez = int.Parse(rez1);
                rez = Math.Abs(rez);
            }
            catch
            {
                Console.WriteLine("\nНевірно введені дані");
                Thread.Sleep(3000);
                //menu_admin(login, pasw, false, h);
                return;
            }

            if (rez1[0] == '+')
            {
                edit_azc(misto,rez,login,pasw);
            }

        }
        public static void menu_admin(string login, string pasw, bool povtor = false,char vubor_pod_menu=' ')
        {
            if (povtor == false)
            {
                hello(login);
            }
            else
            {
                Console.WriteLine();
            }

            char h=' ';
            if (vubor_pod_menu ==' ')
            {
                Console.WriteLine("====================== МЕНЮ ========================");
                Console.WriteLine(" 1 -> Показати всі міста");
                Console.WriteLine(" 2 -> Показати всих користувачів");
                Console.WriteLine(" 0 -> Вихід");
                Console.Write("\n\tЗробіть свій вибір: ");

                while (true)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                    h = keyInfo.KeyChar;
                    if (h == '0' || h == '1' || h == '2' || h == '3')
                    {
                        Console.Write(h);
                        break;
                    }
                }
            }
            else
            {
                h = vubor_pod_menu;
            }
            

            switch (h)
            {
                case '0':
                    StartMenu();
                    break;
                case '1':
                    hello(login);
                    Console.WriteLine("---Всі міста в базі");
                    pokaz_vsi_mista(login);

                    Console.WriteLine("\n================== МЕНЮ =====================");
                    Console.WriteLine("  +  -> Додати місто");
                    Console.WriteLine(" +ID -> Редагувати назву міста");
                    Console.WriteLine(" -ID -> Видалити місто та дані з ним пов'язані");
                    Console.WriteLine("  ID -> Показати всі АЗС міста");
                    break;
                case '2':
                    all_users(login);
                    break;

            }

            Console.WriteLine("  0  -> До попереднього меню");
            Console.Write("\n\tЗробіть свій вибір: ");

            string ymova;
            int rez;
            ymova = @"^[\+\-\d]";
            string rez1 = vvod_str(ymova);
            if (rez1 == "0")
            {
                menu_admin(login, pasw);
                return;
            }
            else if (rez1 == "+")
            {
                mew_misto(login, pasw);
                return;
            }

            try
            {
                rez = int.Parse(rez1);
                rez=Math.Abs(rez);
            }
            catch
            {
                Console.WriteLine("\nНевірно введені дані");
                Thread.Sleep(3000);
                menu_admin(login, pasw, false, h);
                return;
            }

            if (h == '1')
            {
                    using (var context = new GasStationContext())
                    {
                        // Шукаємо місто по ИД
                        var location = context.Locations.FirstOrDefault(u => u.Id == rez);

                        if (location == null)
                        {
                            Console.WriteLine("\nМісто з таким ID не знайдено");
                            Thread.Sleep(3000);
                            menu_admin(login, pasw, false, h);
                            return;
                        }

                        if (rez1[0] == '+')
                        {
                            edit_misto(rez,login,pasw);
                            return;
}
                        else if (rez1[0] == '-')
                        {
                            del_misto(rez,login,pasw);
                            return;
                        }
                        else
                        {
                            hello(login);
                            string misto = location.Name;
                            bool rez2=pokaz_kolonku_mista(misto,login);

                            menu_admin_AZS(login, pasw,misto,rez2);
                        }
                    }    
            }
            else if (h == '2')
            {

            }
        }
        public static void StartMenu()
        {
            Console.Clear();
            Console.WriteLine("Вітаємо Вас на Автозаправці самообслуговування!");

            int kol = 0;
            int rez;
            string firstName, password;
            while (true)
            {
                Console.Write("\n\tВведіть свій логін: ");
                string ymova = @"^[a-zA-z\d]+$";
                firstName = vvod_str(ymova);

                Console.Write("\n\tВведіть пароль: ");
                password = vvod_str(ymova);

                rez = prov_users(firstName, password);
                if ( rez!=-1)
                {
                    break;
                }
                Console.WriteLine("\nНевірний логін чи пароль");
                kol++;
                if (kol == 3)
                {
                    Console.WriteLine("\tВи вичерпали всі спроби.\n");
                    Environment.Exit(0);
                }
            }

            if (rez == 1)
            {
                menu_user(firstName, password);
            }
            else
            {
                menu_admin(firstName, password);
            }
            
        }
        public static void Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.InputEncoding = Encoding.GetEncoding("windows-1251");
            Console.OutputEncoding = Encoding.UTF8;

            using (var db = new GasStationContext())
            {
                // Перевіряємо, чи існує база даних і створюємо її, якщо немає
                db.Database.EnsureCreated();

                // Ініціалізація початкових даних
                GasStationContext.InitializeDatabase(db);

                StartMenu();
            }
        }

        
    }
}
