using TestingTransbank.Services;

namespace TestingTransbank.Managers
{
    public class VariableManager
    {

        private readonly VariableService _variableService;

        public VariableManager(VariableService variableService) 
        {
        
            _variableService = variableService;

        }

        public async Task<Variable?> GetVariablesAsync()
        {

            return await _variableService.GetVariablesAsync();

        }

    }

}
