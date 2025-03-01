using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Example101.Access.Entity.Service.Repository;

namespace Example101.Access.Entity.Service.Repository
{
    public class CreatureRepository : ICreatureRepository
    {
        public IEnumerable<T> Load<T>()  where T : new()
        {
            var t =  new T();
            return [t];
        }
    }
}