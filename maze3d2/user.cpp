#include <stdio.h>
#include <windows.h>

int pins[14]; //bool
unsigned long millis100 = 0;
unsigned int scanValue = 0;

void pinMode(int a, int b)
{
    return;
}

void digitalWrite(int a, int b)
{
   pins[a] = b;
   return;
}

unsigned int digitalRead(int a)
{
   return pins[a];
}

void analogWrite(int a, int b)
{
   return;
}

unsigned int analogRead(int a)
{
   return 1;
}

void bitClear(unsigned int *a, int b)
{

   (*a) &= ~(1 << b);
   return;
}

void bitSet(unsigned int *a, int b)
{
   (*a) |= 1 << b;
   return;
}

unsigned int bitRead(unsigned int a, int b)
{
   unsigned int x;
   a &= 1 << b;
   a >>= b;
   return a;
}

void bitStackClear(unsigned long *a, int b)
{

   *a &= ~(1 << b);
   return;
}

void bitStackSet(unsigned long *a, int b)
{
   (*a) |= 1 << b;
   return;
}

unsigned long millis(void)
{
   return millis100;
}

bool keydown(int key)
{
    return (GetAsyncKeyState(key) & 0x8000) != 0;
}

int getInputsFromFile() {
    FILE *file;

    // Open the file in read mode
    file = fopen("inputsX.txt", "r");
    if (file == NULL) {
        perror("Error opening file");
        return 1;
    }

    pins[0] = fgetc(file) - 48;
    pins[1] = fgetc(file) - 48;
    pins[2] = fgetc(file) - 48;
    pins[3] = fgetc(file) - 48;

    // Close the file
    fclose(file);

    return 0;
}

int putOutputsToFile() {
    FILE *file;

    // Open the file in read mode
    file = fopen("outputsY.txt", "w");
    if (file == NULL) {
        perror("Error opening file");
        return 1;
    }

    fputc(pins[4] + 48, file);
    fputc(pins[5] + 48, file);
    fputc(pins[6] + 48, file);
    fputc(pins[7] + 48, file);

    // Close the file
    fclose(file);

    return 0;
}

int getKeys() //main()
{
    keydown(0x30) ? pins[0] = 1 : pins[0] = 0;
    keydown(0x31) ? pins[1] = 1 : pins[1] = 0;
    keydown(0x32) ? pins[2] = 1 : pins[2] = 0;
    keydown(0x33) ? pins[3] = 1 : pins[3] = 0;

    return 0;
}

void debug(void)
{
   printf("X: %2d%2d%2d%2d | Y: %2d%2d%2d%2d\r", pins[0], pins[1], pins[2], pins[3],
                                                 pins[4], pins[5], pins[6], pins[7]);
}

void debugTimer(void)
{
   printf("%3d%3d%3d%3d  %3d%3d%3d%3d  %5ld\r", pins[0], pins[1], pins[2], pins[3],
                                                 pins[4], pins[5], pins[6], pins[7], millis100);
}
void setupPLC(void)
{
   int i;
   for(i=0; i<14; i++)
       pins[i] = 0;
}

void systemPLC()
{
      debugTimer();

      putOutputsToFile();

      // TICKING
      Sleep(100);
      millis100++;
      // BUTTONS
      //getKeys();       //getButtons();

      getInputsFromFile();
}
