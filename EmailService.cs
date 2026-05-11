using System.Net.Mail;
using System.Net;

namespace Water_Filtration
{
    public class EmailService
    {


        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //public async Task SendEmailAsync(string toEmail, List<WastewaterReportItem> reportItems)
        //{

        //    var smtpSettings = _configuration.GetSection("SmtpSettings");

        //    var fromEmail = smtpSettings["FromEmail"];
        //    var smtpHost = smtpSettings["Host"];
        //    var smtpPort = int.Parse(smtpSettings["Port"]);
        //    var smtpUser = smtpSettings["Username"];
        //    var smtpPass = smtpSettings["Password"];

        //    var subject = "Daily waste water usage report. (Crawley) ";
        //    var body = GenerateEmailBody(reportItems);

        //    using (var client = new SmtpClient(smtpHost, smtpPort))
        //    {
        //        client.Credentials = new NetworkCredential(smtpUser, smtpPass);
        //        client.EnableSsl = true;

        //        var mailMessage = new MailMessage
        //        {
        //            From = new MailAddress(fromEmail),
        //            Subject = subject,
        //            Body = body,
        //            IsBodyHtml = true
        //        };
        //        mailMessage.To.Add(toEmail);

        //        await client.SendMailAsync(mailMessage);
        //    }
        //}


        //public async Task SendEmailAsync(List<string> toEmails, List<WastewaterReportItem> reportItems)
        //{
        //    var smtpSettings = _configuration.GetSection("SmtpSettings");
        //  //  var smtpSettings = _configuration.GetSection("SmtpSettings");
        //    var defaultRecipients = smtpSettings.GetSection("DefaultRecipients").Get<List<string>>();

        //    var fromEmail = smtpSettings["FromEmail"];
        //    var smtpHost = smtpSettings["Host"];
        //    var smtpPort = int.Parse(smtpSettings["Port"]);
        //    var smtpUser = smtpSettings["Username"];
        //    var smtpPass = smtpSettings["Password"];

        //    var subject = "Daily waste water usage report. (Crawley)";
        //    var body = GenerateEmailBody(reportItems);

        //    using (var client = new SmtpClient(smtpHost, smtpPort))
        //    {
        //        client.Credentials = new NetworkCredential(smtpUser, smtpPass);
        //        client.EnableSsl = true;

        //        var mailMessage = new MailMessage
        //        {
        //            From = new MailAddress(fromEmail),
        //            Subject = subject,
        //            Body = body,
        //            IsBodyHtml = true
        //        };

        //        foreach (var email in toEmails)
        //        {
        //            mailMessage.To.Add(email);
        //        }

        //        await client.SendMailAsync(mailMessage);
        //    }
        //}

        public async Task SendEmailAsync(List<WastewaterReportItem> reportItems)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");

            var defaultRecipients = smtpSettings.GetSection("DefaultRecipients").Get<List<string>>();
            var fromEmail = smtpSettings["FromEmail"];
            var smtpHost = smtpSettings["Host"];
            var smtpPort = int.Parse(smtpSettings["Port"]);
            var smtpUser = smtpSettings["Username"];
            var smtpPass = smtpSettings["Password"];

            var subject = "Daily waste water usage report. (Crawley)";
            var body = GenerateEmailBody(reportItems);

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                foreach (var email in defaultRecipients)
                {
                    mailMessage.To.Add(email);
                }

                await client.SendMailAsync(mailMessage);
            }
        }



        public class WastewaterReportItem
        {
            public DateTime Date { get; set; }
            public double WastewaterVolume { get; set; }
            public double FwBwVolume { get; set; }
            public double Permit { get; set; }
            public double Drain { get; set; }
            public double WO { get; set; }
            public double RecoveryPercentage { get; set; }
        }




        public static DateTime GetLondonTime()
        {
            TimeZoneInfo londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, londonTimeZone);
        }

        private string GenerateEmailBody(List<WastewaterReportItem> reportItems)
        {
            DateTime londonTime = GetLondonTime();
            //string formattedDateTime = londonTime.ToString("MM/dd/yyyy hh:mm tt");

            var rowsHtml = !reportItems.Any()
     ? @"<tr><td colspan='7' style='text-align:center;'>No data</td></tr>"
     : string.Join("", reportItems.Select(item => $@"
        <tr>
            <td>{item.Date:MM/dd/yyyy}</td>
            <td>{item.WastewaterVolume:F2}</td>
            <td>{item.FwBwVolume:F2}</td>
            <td>{item.Permit:F2}</td>
            <td>{item.Drain:F2}</td>
            <td>{item.WO:F2}</td>
            <td>{item.RecoveryPercentage:F2}%</td>
        </tr>"));

            return $@"
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <style>
        body {{
            margin: 0;
            padding: 0;
            background-color: #f9f9f9;
            font-family: Arial, sans-serif;
        }}

        .container {{
            width: 100%;
            max-width: 700px;
            margin: auto;
            background-color: #fff;
            border: 1px solid #ddd;
            border-radius: 8px;
            overflow-x: auto;
        }}

        .header {{
            background-color: #4398D3;
            color: white;
            padding: 10px;
            text-align: center;
            font-size: 18px;
            font-weight: bold;
        }}

        .message {{
            font-size: 16px;
            padding: 20px;
            text-align: center;
        }}

        .order-table {{
            width: 100%;
            border-collapse: collapse;
            margin: 0;
            overflow-x: auto;
        }}

        .order-table th, .order-table td {{
            border: 1px solid #ccc;
            padding: 8px;
            text-align: center;
            font-size: 14px;
        }}

        .order-table th {{
            background-color: #e0e0e0;
        }}

        @media (min-width: 577px) and (max-width:767px) {{
            .message {{
                font-size: 12px;
                padding: 15px;
            }}

            .header {{
                font-size: 14px;
            }}

            .order-table th, .order-table td {{
                font-size: 10px;
                padding: 5px;
            }}
        }}

@media (min-width: 375px) and (max-width:576px) {{
            .message {{
                font-size: 12px;
                padding: 15px;
            }}

            .header {{
                font-size: 14px;
            }}

            .order-table th, .order-table td {{
                font-size: 10px;
                padding: 5px;
            }}
        }}

@media (min-width: 300px) and (max-width:374px) {{
            .message {{
                font-size: 10px;
                padding: 15px;
            }}

            .header {{
                font-size: 12px;
            }}

            .order-table th, .order-table td {{
                font-size: 7px;
                padding: 1px;
            }}
        }}

    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>Waste Water Usage Report</div>
        <div class='message'>
            
        </div>
        <table class='order-table'>
            <tr>
                <th>Date</th>
                <th>Wastewater</th>
                <th>FW/BW</th>
                <th>Permeate</th>
                <th>Drain</th>
                <th>WO</th>
                <th>Wastewater Recovery</th>
            </tr>
            {rowsHtml}
        </table>
    </div>
</body>
</html>";
        }



    }
}

//<strong>{formattedDateTime}</strong>.


