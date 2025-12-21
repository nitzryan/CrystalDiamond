#include <torch/torch.h>
#include <iostream>

#define NUM_LEVELS 8

at::Tensor getOutputHitterStats(const at::Tensor& lengths, const at::Tensor& mlbIds, const at::Tensor& dates, const at::Tensor& pt, const at::Tensor& stats, const at::Tensor& pos, float modelId, float modelIdx)
{
  // Ensure valid input data
  TORCH_CHECK(dates.device().type() == at::DeviceType::CPU);
  TORCH_CHECK(pt.device().type() == at::DeviceType::CPU);
  TORCH_CHECK(stats.device().type() == at::DeviceType::CPU);
  TORCH_CHECK(pos.device().type() == at::DeviceType::CPU);
  TORCH_CHECK(lengths.device().type() == at::DeviceType::CPU);
  TORCH_CHECK(mlbIds.device().type() == at::DeviceType::CPU);

  TORCH_CHECK(pt.dim() == 4);
  TORCH_CHECK(stats.dim() == 4);
  TORCH_CHECK(pos.dim() == 4);
  TORCH_CHECK(dates.dim() == 3);
  TORCH_CHECK(lengths.dim() == 1)

  int numPlayers = dates.sizes()[0];
  TORCH_CHECK(pt.sizes()[0] == numPlayers);
  TORCH_CHECK(stats.sizes()[0] == numPlayers);
  TORCH_CHECK(pos.sizes()[0] == numPlayers);
  TORCH_CHECK(lengths.sizes()[0] == numPlayers);
  TORCH_CHECK(mlbIds.sizes()[0] == numPlayers);

  TORCH_CHECK(pt.sizes()[2] == NUM_LEVELS);
  TORCH_CHECK(stats.sizes()[2] == NUM_LEVELS);
  TORCH_CHECK(pos.sizes()[2] == NUM_LEVELS);
  TORCH_CHECK(dates.sizes()[2] == 2);

  // Create output tensor
  int datesSize = dates.sizes()[2];
  int ptSize = pt.sizes()[3];
  int posSize = pos.sizes()[3];
  int statsSize = stats.sizes()[3];
  int outputSize = datesSize + 4 + ptSize + posSize + statsSize; // 4 represents mlbId, modelId, modelIdx, levelId

  int ptOffset = 6;
  int statOffset = ptOffset + ptSize;
  int posOffset = statOffset + statsSize;

  // Get total lengths
  int l = 0;
  for (int i = 0; i < numPlayers; i++)
  {
    l += lengths[i].item<int64_t>();
  }

  auto pt_acc = pt.accessor<float, 4>();
  auto stats_acc = stats.accessor<float, 4>();
  auto pos_acc = pos.accessor<float, 4>();

  // Loop through data to determine length
  int goodElementsCount = 0;
  for (int i = 0; i < numPlayers; i++)
  {
    int playerLength = lengths[i].item<int64_t>();
    for (int j = 0; j < playerLength; j++)
    {
      for (int k = 0; k < NUM_LEVELS; k++)
      {
        float pa = pt_acc[i][j][k][0];
        if (pa > 100)
        {
          goodElementsCount++;
        }
      }
    }
  }

  at::Tensor results = at::zeros({goodElementsCount, outputSize}, at::dtype(at::kFloat));
  auto results_acc = results.accessor<float, 2>();

  int n = 0;
  for (int i = 0; i < numPlayers; i++)
  {
    int playerLength = lengths[i].item<int64_t>();
    float mlbId = static_cast<float>(mlbIds[i][0].item<int64_t>());
    for (int j = 0; j < playerLength; j++)
    {
      float year = static_cast<float>(dates[i][j][0].item<int64_t>());
      float month = static_cast<float>(dates[i][j][1].item<int64_t>());
      for (int k = 0; k < NUM_LEVELS; k++)
      {
        float pa = pt_acc[i][j][k][0];
        if (pa > 100)
        {
          results[n][0] = mlbId;
          results[n][1] = modelId;
          results[n][2] = modelIdx;
          results[n][3] = year;
          results[n][4] = month;
          results[n][5] = static_cast<float>(k);
          for (int m = 0; m < ptSize; m++)
          {
            results_acc[n][ptOffset + m] = pt_acc[i][j][k][m];
          }
          for (int m = 0; m < statsSize; m++)
          {
            results_acc[n][statOffset + m] = stats_acc[i][j][k][m];
          }
          for (int m = 0; m < posSize; m++)
          {
            results_acc[n][posOffset + m] = pos_acc[i][j][k][m];
          }

          n++;
        }
      }
    }
  }

  return results;
}

at::Tensor getOutputPitcherStats(const at::Tensor& lengths, const at::Tensor& mlbIds, const at::Tensor& dates, const at::Tensor& pt, const at::Tensor& stats, const at::Tensor& pos, float modelId, float modelIdx)
{
  // Ensure valid input data
  TORCH_CHECK(dates.device().type() == at::DeviceType::CPU);
  TORCH_CHECK(pt.device().type() == at::DeviceType::CPU);
  TORCH_CHECK(stats.device().type() == at::DeviceType::CPU);
  TORCH_CHECK(pos.device().type() == at::DeviceType::CPU);
  TORCH_CHECK(lengths.device().type() == at::DeviceType::CPU);
  TORCH_CHECK(mlbIds.device().type() == at::DeviceType::CPU);

  TORCH_CHECK(pt.dim() == 4);
  TORCH_CHECK(stats.dim() == 4);
  TORCH_CHECK(pos.dim() == 4);
  TORCH_CHECK(dates.dim() == 3);
  TORCH_CHECK(lengths.dim() == 1)

  int numPlayers = dates.sizes()[0];
  TORCH_CHECK(pt.sizes()[0] == numPlayers);
  TORCH_CHECK(stats.sizes()[0] == numPlayers);
  TORCH_CHECK(pos.sizes()[0] == numPlayers);
  TORCH_CHECK(lengths.sizes()[0] == numPlayers);
  TORCH_CHECK(mlbIds.sizes()[0] == numPlayers);

  TORCH_CHECK(pt.sizes()[2] == NUM_LEVELS);
  TORCH_CHECK(stats.sizes()[2] == NUM_LEVELS);
  TORCH_CHECK(pos.sizes()[2] == NUM_LEVELS);
  TORCH_CHECK(dates.sizes()[2] == 2);

  // Create output tensor
  int datesSize = dates.sizes()[2];
  int ptSize = pt.sizes()[3];
  int posSize = pos.sizes()[3];
  int statsSize = stats.sizes()[3];
  int outputSize = datesSize + 4 + ptSize + posSize + statsSize; // 4 represents mlbId, modelId, modelIdx, levelId

  int ptOffset = 6;
  int statOffset = ptOffset + ptSize;
  int posOffset = statOffset + statsSize;

  // Get total lengths
  int l = 0;
  for (int i = 0; i < numPlayers; i++)
  {
    l += lengths[i].item<int64_t>();
  }

  auto pt_acc = pt.accessor<float, 4>();
  auto stats_acc = stats.accessor<float, 4>();
  auto pos_acc = pos.accessor<float, 4>();

  // Loop through data to determine length
  int goodElementsCount = 0;
  for (int i = 0; i < numPlayers; i++)
  {
    int playerLength = lengths[i].item<int64_t>();
    for (int j = 0; j < playerLength; j++)
    {
      for (int k = 0; k < NUM_LEVELS; k++)
      {
        float outs_sp = pt_acc[i][j][k][0];
        float outs_rp = pt_acc[i][j][k][1];
        if (outs_sp + outs_rp > 60)
        {
          goodElementsCount++;
        }
      }
    }
  }

  at::Tensor results = at::zeros({goodElementsCount, outputSize}, at::dtype(at::kFloat));
  auto results_acc = results.accessor<float, 2>();

  int n = 0;
  for (int i = 0; i < numPlayers; i++)
  {
    int playerLength = lengths[i].item<int64_t>();
    float mlbId = static_cast<float>(mlbIds[i][0].item<int64_t>());
    for (int j = 0; j < playerLength; j++)
    {
      float year = static_cast<float>(dates[i][j][0].item<int64_t>());
      float month = static_cast<float>(dates[i][j][1].item<int64_t>());
      for (int k = 0; k < NUM_LEVELS; k++)
      {
        float outs_sp = pt_acc[i][j][k][0];
        float outs_rp = pt_acc[i][j][k][1];
        if (outs_sp + outs_rp > 60)
        {
          results[n][0] = mlbId;
          results[n][1] = modelId;
          results[n][2] = modelIdx;
          results[n][3] = year;
          results[n][4] = month;
          results[n][5] = static_cast<float>(k);
          for (int m = 0; m < ptSize; m++)
          {
            results_acc[n][ptOffset + m] = pt_acc[i][j][k][m];
          }
          for (int m = 0; m < statsSize; m++)
          {
            results_acc[n][statOffset + m] = stats_acc[i][j][k][m];
          }
          for (int m = 0; m < posSize; m++)
          {
            results_acc[n][posOffset + m] = pos_acc[i][j][k][m];
          }

          n++;
        }
      }
    }
  }

  return results;
}

PYBIND11_MODULE(EvalStats, m) {
  m.def("getOutputHitterStats", &getOutputHitterStats, "Gets Hitter Output Tensor");
  m.def("getOutputPitcherStats", &getOutputPitcherStats, "Gets Pitcher Output Tensor");
}