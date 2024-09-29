using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealSync.Domain.Entities;

public class BaseEntity
{
    public long? CreatedBy { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public long? UpdatedBy { get; set; }

    public DateTimeOffset UpdatedDate { get; set; }
}