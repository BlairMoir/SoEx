using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Example101.Access.Entity.Service.Repository.Creatures
{
    public record Whale
    {
        public Guid Id {get; init;} =  Guid.NewGuid();
        public string Source {get; init; } = "Concrete";
    }
}