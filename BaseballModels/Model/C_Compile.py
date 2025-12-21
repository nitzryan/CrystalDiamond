from setuptools import setup
from torch.utils import cpp_extension

setup(name="EvalStats",
      ext_modules=[
          cpp_extension.CppExtension(
            "EvalStats",
            ["Eval_Stats.cpp"],
            )
          ],
      cmdclass={'build_ext': cpp_extension.BuildExtension},
)

