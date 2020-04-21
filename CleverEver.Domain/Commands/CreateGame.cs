using CleverEver.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleverEver.Commands
{
    class CreateGame : ICommand
    {
        public Guid Id { get; }

        public CreateGame()
        {
            Id = Guid.NewGuid();
        }
      
    }
}
