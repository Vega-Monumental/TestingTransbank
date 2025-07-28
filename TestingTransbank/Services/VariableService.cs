using SalidaAutomaticaQR.Models;

namespace TestingTransbank.Services
{

    public class VariableService
    {

        private readonly VariableRepository _variableRepository;

        public VariableService(VariableRepository variableRepository)
        {

            _variableRepository = variableRepository;

        }

        public async Task<Variable?> GetVariablesAsync()
        {

            return await _variableRepository.GetVariablesAsync();

        }

    }

}
