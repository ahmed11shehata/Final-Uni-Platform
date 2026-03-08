using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Shared.Dtos.Info_Module.AssignmentDto
{
    public class SubmitAssignmentDto
    {
        public int AssignmentId { get; set; }

        public IFormFile File { get; set; }
    }
}
