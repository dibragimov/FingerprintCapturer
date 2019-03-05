using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HorioFingerprintCapturer
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Template { get; set; }
        public string Display
        {
            get
            {
                return FirstName + " " + LastName + " - " + Code;
            }
        }
    }
}
