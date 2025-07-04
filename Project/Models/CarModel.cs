﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Project.Models;

public partial class CarModel
{
    public ulong Id { get; set; }

    public string ModelName { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CarMarkModelCountry> CarMarkModelCountries { get; set; } = new List<CarMarkModelCountry>();

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}