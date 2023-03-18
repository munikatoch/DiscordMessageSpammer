using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.MlTrainer
{
    public interface IMlModelTrainer
    {
        void TrainerModel(bool isDeleteWorkspaceAndModel, bool isModelTrainAgain);
    }
}
