﻿namespace mega_bank_corp_tpl_demo
{
    public class ApplicantDetails
    {
        public string NationalIdNumber { get; }

        public ApplicantDetails(string nationalIdNumber)
        {
            NationalIdNumber = nationalIdNumber;
        }
    }
}