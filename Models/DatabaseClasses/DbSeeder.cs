namespace Cinema.Models.DatabaseClasses;

public static class DbSeeder
{
    public static void Seed(Database db)
    {
        var age12 = db.AgeLimits.FirstOrDefault(a => a.Value == 12) ?? new AgeLimit { Name = "12+", Value = 12 };
        var age16 = db.AgeLimits.FirstOrDefault(a => a.Value == 16) ?? new AgeLimit { Name = "16+", Value = 16 };
        var age18 = db.AgeLimits.FirstOrDefault(a => a.Value == 18) ?? new AgeLimit { Name = "18+", Value = 18 };

        var action = db.Genres.FirstOrDefault(g => g.Name == "Боевик")
            ?? new Genre { Name = "Боевик", Description = "Динамичный жанр с трюками и погонями" };
        var scifi = db.Genres.FirstOrDefault(g => g.Name == "Фантастика")
            ?? new Genre { Name = "Фантастика", Description = "Миры будущего и научные технологии" };
        var comedy = db.Genres.FirstOrDefault(g => g.Name == "Комедия")
            ?? new Genre { Name = "Комедия", Description = "Лёгкое развлекательное кино" };
        var horror = db.Genres.FirstOrDefault(g => g.Name == "Ужасы")
            ?? new Genre { Name = "Ужасы", Description = "Остросюжетные фильмы с элементами страха" };

        var locRu = new Localisation { Name = "Русский дубляж" };
        var locOrig = new Localisation { Name = "Оригинал + субтитры" };

        var seatNormal = db.SeatTypes.FirstOrDefault(s => s.Name == "Обычный")
            ?? new SeatType { Name = "Обычный", BasePriceModifier = 1.0 };
        var seatVip = db.SeatTypes.FirstOrDefault(s => s.Name == "VIP")
            ?? new SeatType { Name = "VIP", BasePriceModifier = 1.5 };

        if (!db.Cinemas.Any())
        {
            db.Cinemas.Add(new Cinema
            {
                Name = "Cinema Lives Inside",
                Adress = "ул. Центральная, 1",
                Description = "Учебный кинотеатр с комфортными залами",
                ContactInformations =
                [
                    new ContactInformation
                    {
                        Position = "Администратор",
                        Email = "info@cinema.local",
                        PhoneNumber = "+7 (999) 000-00-00"
                    }
                ]
            });
        }

        var hall1 = db.Halls.FirstOrDefault(h => h.Number == 1)
            ?? CreateHall(1, rowsCount: 5, seatsPerRow: 8, vipRow: 5, seatNormal, seatVip);
        var hall2 = db.Halls.FirstOrDefault(h => h.Number == 2)
            ?? CreateHall(2, rowsCount: 6, seatsPerRow: 10, vipRow: 6, seatNormal, seatVip);

        var movies = new List<Movie>
        {
            new()
            {
                Name = "Аватар: Путь воды",
                Description = "Продолжение истории Джейка Салли и Нейтири, которые защищают свою семью и океаны Пандоры.",
                Timing = new TimeSpan(3, 12, 0),
                TrailerPath = "",
                PosterPath = "Аватар_Путь_воды_постер.jpg",
                AgeLimit = age12,
                Genres = [scifi, action],
                Localisations = [locRu, locOrig]
            },
            new()
            {
                Name = "Kingsman: Секретная служба",
                Description = "Юный хулиган получает шанс стать агентом элитной независимой разведки.",
                Timing = new TimeSpan(2, 9, 0),
                TrailerPath = "",
                PosterPath = "Kingsman_The_Secret_Service.jpg",
                AgeLimit = age16,
                Genres = [action, comedy],
                Localisations = [new Localisation { Name = "Русский дубляж" }]
            },
            new()
            {
                Name = "Люди в чёрном",
                Description = "Секретное агентство контролирует пребывание инопланетян на Земле.",
                Timing = new TimeSpan(1, 38, 0),
                TrailerPath = "",
                PosterPath = "Men_in_Black_Poster.jpg",
                AgeLimit = age12,
                Genres = [scifi, comedy],
                Localisations = [new Localisation { Name = "Русский дубляж" }]
            },
            new()
            {
                Name = "Кошмар на улице Вязов",
                Description = "Классический хоррор о Фредди Крюгере, преследующем подростков во снах.",
                Timing = new TimeSpan(1, 31, 0),
                TrailerPath = "",
                PosterPath = "Nightmare_On_Elm_Street.jpg",
                AgeLimit = age18,
                Genres = [horror],
                Localisations = [new Localisation { Name = "Русский дубляж" }]
            },
            new()
            {
                Name = "Индиана Джонс",
                Description = "Новые приключения знаменитого археолога.",
                Timing = new TimeSpan(2, 15, 0),
                TrailerPath = "",
                PosterPath = "Indiana_Jones_And_A_Temple_Of_Faith.jpg",
                AgeLimit = age12,
                Genres = [action],
                Localisations = [new Localisation { Name = "Русский дубляж" }]
            },
            new()
            {
                Name = "Очень страшное кино",
                Description = "Пародия на популярные фильмы ужасов и триллеры.",
                Timing = new TimeSpan(1, 28, 0),
                TrailerPath = "",
                PosterPath = "Scary_Movie.jpg",
                AgeLimit = age18,
                Genres = [comedy],
                Localisations = [new Localisation { Name = "Русский дубляж" }]
            },
            new()
            {
                Name = "Крушащая машина",
                Description = "Биографическая драма о бойце ММА Марке Керре.",
                Timing = new TimeSpan(2, 0, 0),
                TrailerPath = "",
                PosterPath = "smashingmachine.jpg",
                AgeLimit = age16,
                Genres = [action],
                Localisations = [new Localisation { Name = "Русский дубляж" }]
            },
            new()
            {
                Name = "Битва за битвой",
                Description = "Остросюжетная история о борьбе и выживании.",
                Timing = new TimeSpan(2, 10, 0),
                TrailerPath = "",
                PosterPath = "Битва_за_битвой.jpg",
                AgeLimit = age16,
                Genres = [action],
                Localisations = [new Localisation { Name = "Русский дубляж" }]
            },
            new()
            {
                Name = "Фильм 1",
                Description = "Описание фильма.",
                Timing = new TimeSpan(1, 50, 0),
                TrailerPath = "",
                PosterPath = "1012209.jpg",
                AgeLimit = age12,
                Genres = [comedy],
                Localisations = [new Localisation { Name = "Русский дубляж" }]
            },
            new()
            {
                Name = "Фильм 2",
                Description = "Описание фильма.",
                Timing = new TimeSpan(1, 45, 0),
                TrailerPath = "",
                PosterPath = "1015178.jpg",
                AgeLimit = age12,
                Genres = [scifi],
                Localisations = [new Localisation { Name = "Русский дубляж" }]
            }
        };

        var existingNames = db.Movies.Select(m => m.Name).ToHashSet();
        var newMovies = movies.Where(m => !existingNames.Contains(m.Name)).ToList();

        if (newMovies.Count == 0)
            return;

        db.Movies.AddRange(newMovies);

        var now = DateTime.Now;
        var today = now.Date;

        foreach (var movie in newMovies)
        {
            db.MovieRentals.Add(new MovieRental
            {
                Movie = movie,
                RentalStart = today.AddDays(-3),
                RentalEnd = today.AddDays(30)
            });

            db.MovieScreenings.AddRange(
                new MovieScreening
                {
                    Movie = movie,
                    Hall = hall1,
                    Price = 350,
                    BeginningTime = today.AddDays(1).AddHours(14)
                },
                new MovieScreening
                {
                    Movie = movie,
                    Hall = hall2,
                    Price = 450,
                    BeginningTime = today.AddDays(1).AddHours(19).AddMinutes(30)
                },
                new MovieScreening
                {
                    Movie = movie,
                    Hall = hall1,
                    Price = 400,
                    BeginningTime = today.AddDays(2).AddHours(18)
                }
            );
        }

        db.SaveChanges();
    }

    private static Hall CreateHall(int number, int rowsCount, int seatsPerRow, int vipRow, SeatType normal, SeatType vip)
    {
        var rows = new List<Row>();
        for (int r = 1; r <= rowsCount; r++)
        {
            var isVip = r == vipRow;
            var places = new List<Place>();
            for (int p = 1; p <= seatsPerRow; p++)
            {
                places.Add(new Place
                {
                    PlaceNumber = p,
                    VIP = isVip,
                    SeatType = isVip ? vip : normal
                });
            }
            rows.Add(new Row { RowNumber = r, Places = places });
        }

        return new Hall { Number = number, Rows = rows };
    }
}
