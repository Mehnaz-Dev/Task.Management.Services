﻿namespace Task.Management.Services.Models
{
    public class VerifyOtpRequest
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}