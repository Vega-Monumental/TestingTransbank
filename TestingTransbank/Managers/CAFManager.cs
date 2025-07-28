using TestingTransbank.Services;

namespace TestingTransbank.Managers
{
    public class CAFManager
    {

        private readonly CAFService _cafService;

        public CAFManager(CAFService cafService)
        {

            _cafService = cafService;

        }

        public async Task<CAF?> GetCAFAsync()
        {

            return await _cafService.GetCAFAsync();

        }

    }

}