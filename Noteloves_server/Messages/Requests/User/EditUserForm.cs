﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noteloves_server.Messages.Requests.User
{
    public class EditUserForm
    {
        public string Name { get; set; }
        public DateTime BirthDay { get; set; }
    }
}
