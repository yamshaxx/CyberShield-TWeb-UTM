using System.Collections.Generic;
using CyberShield.BusinessLogic.Interface;

namespace CyberShield.BusinessLogic.BL_Struct
{
    public class HelpService : IHelpService
    {
        private readonly IErrorHandlingService _errorHandler;

        public HelpService(IErrorHandlingService errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public string GetHelpContent()
        {
            return "This is the Help controller index action. The routing system is working for this controller.";
        }

        public Dictionary<string, string> GetFAQs()
        {
            return new Dictionary<string, string>
            {
                {"Cum pot programa o consultatie?", "Puteti programa o consultatie accesand sectiunea Servicii si completand formularul de programare."},
                {"Cat costa serviciile?", "Preturile variaza in functie de tipul si complexitatea serviciului. Contactati-ne pentru o oferta personalizata."},
                {"Cat dureaza un test de penetrare?", "Durata depinde de complexitatea sistemului, de obicei intre 1-4 saptamani."},
                {"Oferiti rapoarte detaliate?", "Da, oferim rapoarte complete cu vulnerabilitatile identificate si recomandari de remediere."}
            };
        }

        public IEnumerable<string> GetTroubleshootingGuides()
        {
            return new List<string>
            {
                "Ghid pentru configurarea firewall-ului",
                "Proceduri de backup si recovery",
                "Implementarea autentificarii cu doi factori",
                "Monitorizarea traficului de retea",
                "Actualizarea regulata a sistemelor"
            };
        }

        public void LogHelpRequest(string section, string username = null)
        {
            try
            {
                var logMessage = $"Help section accessed: {section}";
                if (!string.IsNullOrEmpty(username))
                {
                    logMessage += $" by user: {username}";
                }
                
                _errorHandler?.LogError(logMessage, "HelpService.LogHelpRequest");
            }
            catch (System.Exception ex)
            {
                _errorHandler?.LogError(ex, "HelpService.LogHelpRequest");
            }
        }
    }
} 