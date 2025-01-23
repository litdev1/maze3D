#include "user.cpp"
#include "plcLib.cpp"

int main()
{
   setupPLC();

    while(!keydown(VK_ESCAPE))
    {
      in(X0);
      out(Y0);

      in(X1);
      out(Y1);

      in(X2);
      out(Y2);

      in(X3);
      out(Y3);

      systemPLC();
   }
}
