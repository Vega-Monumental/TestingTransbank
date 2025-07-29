using TestingTransbank.Repositories;

namespace TestingTransbank.Services
{
    public class CAFService
    {

        private readonly CAFRepository _cafRepository;

        public CAFService(CAFRepository cafRepository)
        { 

            _cafRepository = cafRepository;

        }

        public async Task<CAF?> GetCAFAsync()
        {

            return await _cafRepository.GetCAFAsync();

        }

    }

}
