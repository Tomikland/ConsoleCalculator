using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Testing Github extension

namespace ConsoleCalculator
{
    /*HOW THIS ALL WORKS:
      1.input
      2.find the operator of the first operation that needs to be calculated
      3.construct that operation
      4.calculate it
      5.now insert the result in the place of the operation, have that as the input
      6.repeat until no more operations are left to calculate
      7.profit??

      8.I realized that this could s̶o̶l̶v̶e̶  brute force some equations too, with some changes.
      Maybe. I'll have to see about that.  
      */

    //TODO: Ignore '+' when it means positive
    //TODO: Handle parsing errors & incorrect inputs
    //TODO: Maybe write "-" instead of "+-" in the substeps? It looks nicer.
     

    class Program
    {
        static int indexOffset = 0; //needed because FindOperation() only looks inside the string we passed to it.
        //so the index it returns is the right index in the substring, but not the input string.

        static string input;
        static int opStartIndex;
        static int opEndIndex = -1;

        static void Main(string[] args)
        {
            bool showSteps = true; //show all sub-steps or not, useful for debugging.
            Operation currOp;
            int OpIndex;
            double result;

            while (true)
            {
                input = Console.ReadLine();
                RemoveWhiteSpaces(ref input);
                AddPlusSigns(ref input);
                
                OpIndex = FindOperator(input);
                while (OpIndex > -1)
                {
                    indexOffset = 0; opStartIndex = 0; opEndIndex = -1;
                    OpIndex = FindOperator(input);
                    currOp = GetOperation(OpIndex);
                    result = currOp.Calculate();
                    CorrectIndexes(ref opStartIndex, ref opEndIndex);
                    input = input.Remove(opStartIndex, opEndIndex + 1 - opStartIndex);
                    input = input.Insert(opStartIndex, result.ToString());
                    
                    Console.WriteLine(input);
                    OpIndex = FindOperator(input);
                }
                
                Console.WriteLine();
            }
        }
        
        public static void CorrectIndexes(ref int indS, ref int indE) //opIndex doesn't count the ().
        //Let's correct it so they get removed
        {
            if (indS - 1 >= 0 && input[indS - 1] == '(' && indE + 1 < input.Length && input[indE + 1] == ')')
            {
                //e.g. (1+2)
                indS--;
                indE++;
            }

        }

        public static int FindOperator(string s)//I really love this implementation. This function keeps
                                                //calling itself until we find the first operation to calculate. 
        {
            int cnt = 1; //Not sure of the naming
            int parStartI;
            int parEndI = -1;  //Starts at -1 so that we can check if the input is correct.

            //just a tiny bit of optimization: cacheing the index because we will need it later
            parStartI = s.IndexOf('(');

            //PARENTHESES

            //Are there any parentheses at all?
            if (parStartI != -1) //Apparently, IndexOf() returns -1 if it doesn't find anything 
            {

                //using IndexOf isn't right when we have for example (1+(2*3))+4
                //using LastIndexOf() isn't right when we have for example (1+2)*(3+4)
                //sooo... Find a closed parenthesis
                for (int i = parStartI + 1; i < s.Length; i++)
                {
                    //I feel this is a really clever solution.
                    if (s[i] == '(')
                    {
                        cnt++;
                    }
                    else if (s[i] == ')')
                    {
                        cnt--;
                    }
                    if (cnt == 0)
                    {
                        //We have found a closed parenthesis!!
                        indexOffset += parStartI + 1;
                        parEndI = i;
                        break;
                    }

                }
                if (parEndI == -1)  //quick check: if the for loop above didn't find anything,
                                    //the input doesn't have as many ')' as '('.
                                    //a little thing that probably doesn't matter: extra ')' don't bother the algorithm.
                {
                    return -2;//input error
                }
                string SubStr = s.Substring(parStartI + 1, -1 * (parStartI + 1 - parEndI));

                return FindOperator(SubStr); //feed a substring to another FindOperator()
                                             //the substring contains what we found inside the parentheses

            }

            //MULTIPLICATION & DIVISION

            int temp1; // not the right names but whatever.
            int temp2;

            temp1 = s.IndexOf('*');
            temp2 = s.IndexOf('/');

            //FIXME: I don't like this boolean logic, it looks way too complicated for what it does.
            //I wonder if it can be simplified.

            if ((temp1 != -1 && temp1 < temp2) || (temp2 == -1 && temp1 > -1))
            {
                return (temp1 + indexOffset);
            }
            else if ((temp2 != -1 && temp2 < temp1) || (temp1 == -1 && temp2 > -1))
            {
                return (temp2 + indexOffset);
            }//if temp1 = temp2, they must be -1, so none of them are present.

            //ADDITION

            temp1 = s.IndexOf('+');


            if (temp1 > -1)
            {
                return (s.IndexOf('+') + indexOffset);
            }



            return -1; //we couldn't find an operation.
                       //hopefully, that's because there aren't any left.


        }
        public static void AddPlusSigns(ref string s)
        {
            for (int i = 1; i < s.Length; i++)
            {

                if (s[i] == '-'  )
                {
                    if(s[i - 1] != '(' && s[i-1] != '+' && s[i - 1] != '*' && s[i - 1] != '/')
                        //TODO: use IsOpOrPar for this check maybe? The only difference is the ')'
                        s = s.Insert(i, "+");
                        i++;

                }

            }
        }
        public static Operation GetOperation(int opIndex)
        {

            double l = 0;
            string lString;

            double r = 0;
            string rString;

            Op type = Op.ADD;
            char opChar;

            //Step 1:
            //Operation type
            opChar = input[opIndex];
            switch (opChar)
            {
                case '+':
                    type = Op.ADD;
                    break;
                case '*':
                    type = Op.MULT;
                    break;
                case '/':
                    type = Op.DIV;
                    break;
                default:
                    throw new Exception("Operator type could not be parsed.");
            }
            //Step 2:
            //the two numbers (left and right)
            
            
                lString = input.Substring(GetNumStartIndex(opIndex), opIndex - GetNumStartIndex(opIndex));
                l = double.Parse(lString);
           
                rString = input.Substring(opIndex + 1, GetNumEndIndex(opIndex) - opIndex);
                        
            r = double.Parse(rString);
            return new Operation(l, r, type);
        }
        public static int GetNumStartIndex(int opI)
        {
            for (int i = opI - 1; i >= 0; i--)
            {
                if (IsOpOrPar(input[i]) == true)
                {
                    opStartIndex = i + 1;
                    return opStartIndex;
                }
            }
            return 0;
        }
        public static int GetNumEndIndex(int OpIndex)
        {
            for (int i = OpIndex + 1; i < input.Length; i++)
            {
                if (IsOpOrPar(input[i]) == true)
                {
                    opEndIndex = i - 1;
                    return opEndIndex;
                }
            }
            opEndIndex = input.Length - 1;
            return opEndIndex;
        }
        public static bool IsOpOrPar(char ch)
        {
            if (ch == '+' || ch == '*' || ch == '/' || ch == '(' || ch == ')')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void  RemoveWhiteSpaces(ref string s)
        {
            for (int i = 0; i < s.Length;)
            {
                if (s[i] == ' ')
                {
                    s = s.Remove(i, 1);
                }
                else
                {
                    i++;
                }
            }
        }
    }
    enum Op { ADD, MULT, DIV };
    class Operation
    {
        double l;
        double r;
        Op operationType;

        public Operation(double x, double y, Op type)
        {
            l = x;
            r = y;
            operationType = type;
        }

        public double Calculate()
        {
            double result;
            switch (operationType)
            {
                case Op.ADD:
                    result = l + r;
                    break;
                case Op.MULT:
                    result = l * r;
                    break;
                case Op.DIV:
                    result = l / r;
                    break;
                default:
                    throw new Exception("Undefined operation type");
            }
            return result;
        }
        public string GetString()  //not ToString because it already exist as Object.ToString(). Could probably override but whatever.
        {

            {
                return l.ToString() + operationType.ToString() + r.ToString();
            }

        }
    }
}

