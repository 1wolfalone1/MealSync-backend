﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealSync.Domain.Entities;

[Table("dormitory")]
public class Dormitory : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long LocationId { get; set; }

    public string Name { get; set; }

    public virtual Location Location { get; set; }

    public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();

    public virtual ICollection<ShopDormitory> ShopDormitories { get; set; } = new List<ShopDormitory>();

    public virtual ICollection<ModeratorDormitory> ModeratorDormitories { get; set; } = new List<ModeratorDormitory>();

    public virtual ICollection<DormitoryDistance> DormitoryDistanceTos { get; set; } = new List<DormitoryDistance>();

    public virtual ICollection<DormitoryDistance> DormitoryDistanceFroms { get; set; } = new List<DormitoryDistance>();
}