using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicQueue.Core.Responses.DTOs
{
    public class DoctorResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string OfficeNumber { get; set; } = null!;
        public string WorkSchedule { get; set; } = null!;
        public List<string> Specializations { get; set; } = [];
    }

}
