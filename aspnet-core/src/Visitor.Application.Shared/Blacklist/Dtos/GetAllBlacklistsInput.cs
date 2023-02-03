using System;
using System.Collections.Generic;
using System.Text;
using Visitor.Dto;

namespace Visitor.Blacklist.Dtos
{
    public class GetAllBlacklistsInput: PagedSortedAndFilteredInputDto
    {
        public string Filter { get; set; }
        public string FullNameFilter { get; set; }
        public string IcPassportFilter { get; set; }
        public string PhoneNumberFilter { get; set; }
        public string RemarkFilter { get; set; }

    }
}
