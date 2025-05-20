using CyberShield.BusinessLogic.Interface;

namespace CyberShield.BusinessLogic.BL_Struct
{
    public class DespreService : IDespreService
    {
        private readonly IErrorHandlingService _errorHandler;

        public DespreService(IErrorHandlingService errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public string GetCompanyInfo()
        {
            return @"CyberShield este o companie specializată în securitatea cibernetică, oferind servicii complete 
                     de protecție pentru întreprinderi de toate dimensiunile. Cu o echipă de experți certificați și 
                     experiență vastă în domeniu, ne dedicăm protejării afacerii dumneavoastră împotriva amenințărilor cibernetice moderne.";
        }

        public object GetTeamInfo()
        {
            return new
            {
                TeamMembers = new[]
                {
                    new { Name = "Bivol Dorin", Role = "Cybersecurity Specialist", Certifications = "CISSP, CEH" },
                    new { Name = "Brinzila Calin", Role = "Penetration Testing Expert", Certifications = "OSCP, CISSP" }
                },
                TeamSize = 2,
                YearsOfExperience = 5,
                ProjectsCompleted = 150
            };
        }

        public string GetCompanyHistory()
        {
            return @"Fondată în 2019, CyberShield s-a dezvoltat rapid pentru a deveni unul dintre liderii 
                     în domeniul securității cibernetice din România. Misiunea noastră este să oferim 
                     soluții de securitate de înaltă calitate, adaptate nevoilor specifice ale fiecărui client, 
                     contribuind astfel la construirea unui mediu digital sigur și de încredere.";
        }

        public void LogPageVisit(string section, string username = null)
        {
            try
            {
                var logMessage = $"Despre page section visited: {section}";
                if (!string.IsNullOrEmpty(username))
                {
                    logMessage += $" by user: {username}";
                }
                
                _errorHandler?.LogError(logMessage, "DespreService.LogPageVisit");
            }
            catch (System.Exception ex)
            {
                _errorHandler?.LogError(ex, "DespreService.LogPageVisit");
            }
        }
    }
} 