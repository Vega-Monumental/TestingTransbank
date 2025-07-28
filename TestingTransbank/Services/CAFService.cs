using SalidaAutomaticaQR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
