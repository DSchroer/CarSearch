using System;
using System.Collections.Generic;
using CarSearch.Models;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Linq;

namespace CarSearch.Email
{
    class Notifier
    {
        public void Send(IList<Car> found)
        {
            if (!found.Any())
            {
                return;
            }

            var builder = new StringBuilder();
            AddHeader(builder);
            foreach (var car in found)
            {
                AddCar(builder, car);
            }
            SendEmail(builder.ToString());
        }

        private void AddHeader(StringBuilder builder)
        {
            builder.AppendLine($"Cars found on {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}:");
        }

        private void AddCar(StringBuilder builder, Car car)
        {
            builder.AppendLine();
            builder.AppendLine($"Name: {car.Name}");
            builder.AppendLine($"Price: {car.Price:C}");
            builder.AppendLine($"Transmission: {car.Transmission}");
            builder.AppendLine($"Mileage: {car.Mileage:n0} km");
            builder.AppendLine($"Url: {car.Url}");
        }

        private void SendEmail(string body)
        {
            var fromAddress = new MailAddress("test@example.com", "Car Search");
            var toAddress = new MailAddress("test@example.com");
            const string fromPassword = "";
            const string subject = "Cars Found";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}