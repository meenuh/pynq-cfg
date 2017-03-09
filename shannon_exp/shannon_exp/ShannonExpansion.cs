

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace shannon_exp
{
    /* A token can be an operator or a variable */
    public enum TokenType
    {
        Operator,
        Variable
    };


    /* A single variable or operator in an expression */
    public class Token
    {
        public string word;
        public TokenType type;
        
        public Token(string word, TokenType type)
        {
            this.word = word;
            this.type = type;
        }
    };

    /* Contains an expression in the form "lhs = rhs" */
    public class Expression
    {
        /* The left and right sides of the expression */
        public SubExpression lhs, rhs;

        /* Build a string containing the LHS, assignment operator, and RHS */
        public override string ToString()
        {
            return lhs.ToString() + "=" + rhs.ToString();
        }

        /* Trim input and standardize operators used */
        private string sanitizeInput(string input)
        {
            var output = new List<char>();

            /* Convert expression to use standard operators and trim white space */
            foreach (char src in input)
            {
                switch (src)
                {
                case ' ':   /* Remove all whitespace from input */
                case '\t':
                case '\v':
                    break;

                case '+':   /* Replace alternative operator with standard one */
                    output.Add('|');
                    break;

                case '*':   /* Replace alternative operator with standard one */
                 output.Add('&');
                    break;

                case '!':   /* Replace alternative operator with standard one */
                    output.Add('~');
                    break;

                default:
                    output.Add(src);
                    break;
                }
            }

            /* Return list of characters as a string */
            return String.Join("", output.ToArray());
        }

        public Expression(string input)
        {
            parseString(input);
        }


        /* Parse a string into tokens */
        public void parseString(string input)
        {
            var op_hash = new HashSet<char>("'|&~=");
            var tokens = new List<Token>();

            /* Convert input to standard form */
            input = sanitizeInput(input);

            /* Parse sanitized input and split into variables and operators */
            string literal = "";
            foreach (char ch in input)
            {
                /* Found an operator */
                if (op_hash.Contains(ch))
                {
                    /* If we had been building up a literal, add it as a new token */
                    if (literal != "")
                    {
                        tokens.Add(new Token(literal, TokenType.Variable));
                        literal = "";
                    }

                    /* Add operator as new token */
                    tokens.Add(new Token(String.Join("", ch), TokenType.Operator));
                }
                else
                {
                    /* Found part of a literal */
                    literal += ch;
                }
            }

            /* If there is a literal remaining, add it */
            if (literal != "")
                tokens.Add(new Token(literal, TokenType.Variable));

            /* If postfix negation (') is present, convert to prefix form (~) */
            if (input.Contains('\''))
            {
                for (int index = 0; index < tokens.Count; index++)
                {
                    if (tokens[index].type == TokenType.Operator && tokens[index].word == "'")
                    {
                        /* Remove back-tick */
                        tokens.RemoveAt(index);

                        /* Insert negation operator before token */
                        tokens.Insert(index - 1, new Token("~", TokenType.Operator));
                    }
                }
            }

            /* Split expression into a left subexpression (LHS) and right subexpression (RHS) */
            lhs = new SubExpression(tokens.GetRange(0, 1));
            rhs = new SubExpression(tokens.GetRange(2, tokens.Count - 2));
        }
    }



    /* Represents the left or right hand side of an expression */
    public class SubExpression
    {
        /* Sequence of tokens that define the expression */
        public List<Token> tokens;

        /* List of all unique variables */
        public List<String> lits;

        /* Build a string containing the tokens in this subexpression */
        public override string ToString()
        {
            string temp = "";
            foreach (var token in tokens)
                temp += token.word;
            return temp;
        }

        /* Build subexpression based on token list */
        public SubExpression(List<Token> tokens)
        {
            /* Assign tokens */
            this.tokens = new List<Token>(tokens);

            /* Make hash of unique literals */
            HashSet<string> lit_hash = new HashSet<string>();
            foreach (var token in tokens)
                if (token.type == TokenType.Variable)
                    lit_hash.Add(token.word);

            /* Make list of variables */
            lits = new List<String>(lit_hash.ToArray());
        }
    };



    /* Helper class to represent a minterm */
    public class Minterm
    {
        public List<char> data;
        public bool used;

        public Minterm(Minterm rhs)
        {
            data = new List<char>(rhs.data);
            used = rhs.used;
        }

        public Minterm(Minterm rhs, int position, char replacement)
        {
            data = new List<char>(rhs.data);
            data[position] = replacement;
            used = false;
        }

        public Minterm(string value)
        {
            data = new List<char>(value.ToList());
            used = false;
        }


        /* Test two minterms for equality */
        public override bool Equals(object obj)
        {
            string lhs = String.Join("", data);
            string rhs = String.Join("", (obj as Minterm).data);
            return lhs == rhs;
        }

    
        /* C# specific helper, not used by this code */
        public override int GetHashCode()
        {
            return data.GetHashCode();
        }


        /* Return character array as string */
        public override string ToString()
        {
            return String.Join("", data);
        }
    }


    public class PartitionedMinterms
    {
        public List<List<Minterm>> minterms;    /* Minterms partitioned into three groups: 0, 1, X */
        public List<string> lits;               /* Variables common to all minterms */
        public int control;                            /* Index of control variable used to select partitions */

        /* Build a partitioned minterm inheriting a set of variables */
        public PartitionedMinterms(List<string> lits, int control)
        {
            /* Three partitions for 0, 1, x */
            minterms = new List<List<Minterm>>();
            for (int i = 0; i < 3; i++ )
                minterms.Add(new List<Minterm>());

            /* Variables */
            this.lits = new List<string>(lits);

            /* Control is initially undetermined */
            this.control = control;
        }

        /* Make new partitioned minterms that inherit the variables and control term index of another */
        public PartitionedMinterms(PartitionedMinterms rhs) : this(rhs.lits, rhs.control)
        {
        }
    }



    
    public class ShannonExpansion
    {
        public string input;

        public ShannonExpansion()
        {
        }

        public ShannonExpansion(string input)
        {
            this.input = input;
        }

        public string getLastError()
        {

            return null;
        }

        /* Make binary minterms */
        public List<Minterm> makeMintermsFromExpression(Expression expr)
        {
            var minterms = new List<Minterm>();
            Minterm minterm = null;
            bool invert = false;

            foreach (var token in expr.rhs.tokens)
            {
                /* Make a minterm with as many x's as there are unique variables in the RHS of the expression (e.g. "f=abc+ac" produces "xxx") */
                if (minterm == null)
                   minterm = new Minterm(new String('x', expr.rhs.lits.Count()));

                switch (token.type)
                {
                    case TokenType.Variable:
                        minterm.data[expr.rhs.lits.IndexOf(token.word)] = (invert) ? '0' : '1';
                        invert = false;
                        break;

                    case TokenType.Operator:
                        switch (token.word)
                        {
                            case "|": /* Split sub-expression by OR operator into minterms */
                                minterms.Add(minterm);
                                invert = false;
                                minterm = null;
                                break;

                            case "~":
                                invert = !invert;
                                break;

                            default:
                                invert = false;
                                break;
                        }
                        break;
                }
            }
            if (minterm != null)
                minterms.Add(minterm);

            return minterms;
        }




        /* Split a list of minterms by their control variable into groups where the variable is 0, 1, x */
        public PartitionedMinterms buildPartitionedMinterms(List<Minterm> minterms, List<string> lits, int control)
        {
            var partitions = new PartitionedMinterms(lits, control);

            foreach (Minterm minterm in minterms)
            {
                /* Make new minterm with control term replaced by don't-care */
                Minterm modified = new Minterm(minterm, control, 'x');

                /* Add new minterm to appropriate partition */
                if (minterm.data[control] == '0') partitions.minterms[0].Add(modified);
                else
                if (minterm.data[control] == '1') partitions.minterms[1].Add(modified);
                else
                if (minterm.data[control] == 'x') partitions.minterms[2].Add(modified);
            }

            return partitions;
        }
        


        /* Go through a group of three minterm lists (for control variable = 0, 1, x) and minimize redundant minterms */
        public PartitionedMinterms minimizePartitionedMinterms(PartitionedMinterms partitions)
        {
            if (partitions.control == -1)
                throw new NotImplementedException("Error: minimizePartionedMinterms: No control term defined.\n");

            PartitionedMinterms minimized = new PartitionedMinterms(partitions);

            /* Minimize partitions */
            for (int i = 0; i < 3; i++)
            {
                foreach (Minterm parent in partitions.minterms[i])
                {
                    foreach (Minterm child in partitions.minterms[i])
                    {
                        int diff = 0;
                        int diff_pos = 0;

                        /* Don't compare the same minterm to itself */
                        if (parent == child)
                            continue;

                        /* Count number of differences between parent and child, stop if more than one found */
                        for (int index = 0; index < minimized.lits.Count && diff <= 1; index++)
                        {
                            /* Note position where different occurred */
                            if (parent.data[index] != child.data[index])
                            {
                                diff++;
                                diff_pos = index;
                            }
                        }

                        /* If they differ by one bit, minimize */
                        if (diff == 1)
                        {
                            /* Build minimized minterm */
                            Minterm derived = new Minterm(parent, diff_pos, 'x');

                            /* If we don't already have this minterm, add it and mark parent and child as being reduced */
                            if (!minimized.minterms[i].Contains(derived))
                            {
                                /* Flag parent and child minterms that produced reduced minterm as being used */
                                parent.used = true;
                                child.used = true;

                                minimized.minterms[i].Add(derived);
                                Console.WriteLine("- Reduced ({0}, {1}) -> {3} (@{2})", parent, child, diff, derived);
                            }
                        }
                        else
                        {
                            /* TODO: Find input that can test this code path */
                            // it's this one:
                            ///         input = "f = ~D & E | D & C & A | D & ~C & B"; // lf: maps to two cascaded 2-1 muxes

                            //Console.WriteLine("ERROR: Zero or multiple differences for: P:{0} C:{1} D:{2}", parent, child, diff);
                            //throw new NotImplementedException("Multiple differences");
                        }
                    } /* foreach child */
                } /* foreach parent */

                


                /* Add any minterms we couldn't minimize to the output */
                foreach (Minterm remaining in partitions.minterms[i])
                {
                    if (remaining.used == false && !minimized.minterms[i].Contains(remaining))
                    {
                        Console.WriteLine("- Could not minimize: {0} ({1}), copying to output list.", remaining, remaining.used);
                        minimized.minterms[i].Add(remaining);
                    }
                }
            }

            return minimized;
        }



        //================================================================================================================
        // Functions for building an expression string out of minterms 
        //================================================================================================================

        /* Build a human-readable string from a list of minterms */
        public string buildStringFromMinterms(List<Minterm> minterms, List<string> lits)
        {
            string output = "";
            int max_lits = lits.Count();

            foreach (Minterm minterm in minterms)
            {
                bool wrote = false;
                for (int i = 0; i < max_lits; i++)
                {
                    if (minterm.data[i] != 'x')
                    {
                        if (minterm.data[i] == '0')
                            output += '~';
                        output += lits[i];
                        wrote = true;
                    }
                }

                if (wrote)
                {
                    wrote = false;
                    if (minterm != minterms.Last())
                        output += " | ";
                }
            }
            return output;
        }




        /* Build an expression out of a collection of minterms */
        public string buildStringFromPartitionedMinterms(PartitionedMinterms minimized)
        {
            string output = "";
            int max_lits = minimized.lits.Count();


            /* Output parts of expression that were partitioned by the control term */
            for (int j = 0; j < 2; j++)
            {
                var minterm_list = minimized.minterms[j];   

                /* The first partition is associated with the control term being false */
                if (minterm_list == minimized.minterms[0])
                    output += "~";

                /* Output start of control term */
                output += String.Format("{0}(", minimized.lits[minimized.control]);

                /* Output minterm */
                output += buildStringFromMinterms(minterm_list, minimized.lits);

                /* Output end of control term */
                output +=  ")";

                /* Insert OR term between two partitions */
                if (j == 0)
                    output +=  " | ";
            }

            /* Output remaining parts that are unaffected by the control term */
            foreach (var minterm in minimized.minterms[2])
            {
                string temp = "";
                for(int i = 0; i < max_lits; i++)
                {
                    if (minterm.data[i] == '0') temp += "~" + minimized.lits[i];
                    else
                    if (minterm.data[i] == '1') temp +=       minimized.lits[i];
                }
                if (!String.IsNullOrEmpty(temp))
                    output += " | " + temp;
            }
                
            return output;
        }



        //================================================================================================================
        //================================================================================================================
        //================================================================================================================
        //================================================================================================================
        //================================================================================================================


        class ControlData
        {
            public List<List<int>> term_frequency;
            public int control_frequency;
            public int control;

            /* Calculate frequency that each variable appears in a true or complemented form in a list of minterms */
            public void findVariableFrequency(List<Minterm> minterms, int max_variables)
            {
                const int max_states = 2;

                /* Frequency list has two sub-lists for true and false case */
                term_frequency = new List<List<int>>();
                for (int i = 0; i < max_states; i++)
                    term_frequency.Add(new List<int>());


                /* Make empty frequency list */
                for (int i = 0; i < max_states; i++)
                {
                    term_frequency[i] = new List<int>();
                    for (int j = 0; j < max_variables; j++)
                        term_frequency[i].Add(0);
                }

                /* Calculate term frequency */
                foreach (Minterm minterm in minterms)
                {
                    for (int i = 0; i < max_variables; i++)
                    {
                        if (minterm.data[i] == '0') term_frequency[0][i]++;
                        else
                            if (minterm.data[i] == '1') term_frequency[1][i]++;
                    }
                }
            }

            /* Determine the control variable based on the variable's frequency */
            public bool findControlVariable(/*List<List<int>> term_frequency, ref int control_frequency*/)
            {
                /* Find control term.
                 * This is accomplished by finding the minimum frequency for each variable,
                 * and then locating the largest overall frequency across all variables.
                 */
                control = -1;
                control_frequency = 0;
                for (int j = 0; j < term_frequency[0].Count(); j++)
                {
                    int currentMin = Math.Min(term_frequency[0][j], term_frequency[1][j]);
                    if (currentMin > control_frequency)
                    {
                        control = j;
                        control_frequency = currentMin;
                    }
                }

                return (control != -1);
            }
        }




        public bool expand(string input, ref string result)
        {
            result = null;

            /* Parse string into expression */
            Expression expr = new Expression(input);

            /* Build minterm list from expression */
            var minterms = makeMintermsFromExpression(expr);
            
            /* Calculate frequency of variables used */
            ControlData ctrlData = new ControlData();
            ctrlData.findVariableFrequency(minterms, expr.rhs.lits.Count());

            /* Locate control term */
            if(!ctrlData.findControlVariable())
            {
                /* No control term found. Expression may not be reducible via Shannon expansion */
                Console.WriteLine("No control term found, cannot reduce.");
                Console.ReadKey();
                return true;
            }
            Console.WriteLine("Control term candidate is {0} with frequency {1}.", ctrlData.control, ctrlData.control_frequency);
            Console.WriteLine();


            //============================================================================
            //============================================================================
            /* Debug: Report information */

            /* Print expression */
            Console.WriteLine("Expression: " + expr);

            /* Print all variables used */
            Console.Write("LHS Variables: ");
            foreach (var name in expr.lhs.lits)
                Console.Write("{0}", name);
            Console.WriteLine();

            /* Print all variables used */
            Console.Write("RHS Variables: ");
            foreach (var name in expr.rhs.lits)
                Console.Write("{0}", name);
            Console.WriteLine();

            /* Print finalized expression */
            Console.Write("LHS Tokens: ");
            foreach (var token in expr.lhs.tokens)
                Console.Write("{0}", token.word);
            Console.WriteLine();

            /* Print finalized expression */
            Console.Write("RHS Tokens: ");
            foreach (var token in expr.rhs.tokens)
                Console.Write("{0}", token.word);
            Console.WriteLine();

            /* Print minterms */
            Console.Write("Minterms: ");
            foreach (var term in minterms)
                Console.Write("{0} ", term);
            Console.WriteLine();

            /* Print frequency of terms in minterms */
            for (int i = 0; i < ctrlData.term_frequency.Count; i++)
            {
                Console.Write("Term frequency {0}: ", i);
                foreach (int freq in ctrlData.term_frequency[i])
                    Console.Write("{0} ", freq);
                Console.WriteLine();
            }
            //============================================================================
            //============================================================================

            /* Partition minterms by control signal */
            Console.WriteLine("* Partitioning pass:");
            var partitions = buildPartitionedMinterms(minterms, expr.rhs.lits, ctrlData.control);
               
            /* Display unminimized partitions */
            Console.WriteLine("Partitions:");
            for (int i = 0; i < 3; i++)
            {
                Console.Write("{0}: ", i);
                foreach (var x in partitions.minterms[i])
                    Console.Write("{0} ", x);
                Console.WriteLine();
            }
            Console.WriteLine();

            /* Minimize partitions to remove redundant minterms*/
            Console.WriteLine("* Minimization pass:");
            var minimized = minimizePartitionedMinterms(partitions);
 
            /* Display minimized partitions */
            Console.WriteLine("Minimized partitions:");
            for (int i = 0; i < 3; i++)
            {
                Console.Write("{0}: ", i);
                foreach (var x in minimized.minterms[i])
                    Console.Write("{0} ", x);
                Console.WriteLine();
            }
            Console.WriteLine();

            /* Print final minimized expression */
            Console.WriteLine("* Minimized expression:");         
            Console.WriteLine("Control term: {0}", minimized.lits[minimized.control]);        
            Console.Write("Expression: {0}", buildStringFromPartitionedMinterms(minimized));
            Console.WriteLine();

            /*===============================================================================================*/
            /*===============================================================================================*/
            /*===============================================================================================*/
#if false
            // Second pass

            Console.WriteLine("*************************** Second pass:\n");

            for (int i = 0; i < 3; i++)
            {
                /* Don't try to reduce empty partitions or partitions with one minterm */
                if (minimized.minterms[i].Count <= 1)
                    continue;

                /* Determine variable indices used by this partition */                
                HashSet<int> used = new HashSet<int>();
                foreach (Minterm minterm in minimized.minterms[i])
                {
                    /* Scan minterm string to find true or complemented variables (0 or 1, not x)  */
                    for (int j = 0; j < minterm.data.Count; j++)
                        if (minterm.data[j] != 'x')
                            used.Add(j);
                }

                /* Report variables used by the minterms in this partition */
                string msg = "";
                msg += String.Format("* Partition #{0} minterms use variables: ", i);
                foreach (var index in used)
                    msg += String.Format("{0}, ", minimized.lits[index]);
                Console.WriteLine(msg);


                ControlData cd = new ControlData();
                cd.findVariableFrequency(minimized.minterms[i], minimized.lits.Count);
                if (!cd.findControlVariable())
                {
                    Console.WriteLine("No control term found.");
                    continue;
                }

                Console.WriteLine("Partition {0} uses control term {1}", i, minimized.lits[cd.control]);


                //                var partitions2 = buildPartitionedMinterms(minterms, expr, ctrlData.control);

                 var partitions2 = buildPartitionedMinterms(minimized.minterms[i], minimized.lits, ctrlData.control);

                /* Display unminimized partitions */
                Console.WriteLine("Partitions:");
                for (int ii = 0; ii < 3; ii++)
                {
                    if (partitions2.minterms[ii].Count == 0)
                        continue;
                    Console.Write("{0}: ", ii);
                    foreach (var x in partitions2.minterms[ii])
                        Console.Write("{0} ", x);
                    Console.WriteLine();
                }
                Console.WriteLine();

                /* Minimize partitions to remove redundant minterms*/
                Console.WriteLine("* Minimization pass:");
                var minimized2 = minimizePartitionedMinterms(partitions2);

                /* Display minimized partitions */
                Console.WriteLine("Minimized partitions:");
                for (int ii = 0; ii < 3; ii++)
                {
                    if (minimized2.minterms[ii].Count == 0)
                        continue;
                    Console.Write("{0}: ", ii);
                    foreach (var x in minimized2.minterms[ii])
                        Console.Write("{0} ", x);
                    Console.WriteLine();
                }
                Console.WriteLine();

                /* Print final minimized expression */
                Console.WriteLine("* Minimized expression:");
                Console.WriteLine("Control term: {0}", minimized2.lits[ctrlData.control]);
                Console.Write("Expression: {0}", buildStringFromPartitionedMinterms(minimized2));
                Console.WriteLine();

            }
             
#endif
#if false
            //--------------------------------------------------------
            //--------------------------------------------------------
            //--------------------------------------------------------
            // need to make sub expression based on
            // results, with new lits 
            // 
            // Temporary: hard-code second pass
            // Check if the minterms partitioned by the control term have more than one minterm

            // Output from partition is
            // Minterm list associated with negative control term
            // Minterm list associated with positive control term
            // Minterm list unassociated with control term 
            // To reduce, need literal list used by each minterm list associated with control term,
            // For example for output: 
            // ~D(E) | D(CA | ~CB) | F
            // CA | ~CB needs to have variables A,C,B associated with it
            // These sub-minterms then need to be joined together in the end with all control terms taken into account
            // Should move literal list out of expression and into a structure containing Minterms to 
            // decouple them from the original expression
            //



            // scan both halves of partition for an entry with more than one minterm 
            for (int i = 0; i < 2; i++)
            {
                if (minimized[i].Count > 1)
                {
                    Console.WriteLine("\n** Second pass:\n");
                    Console.WriteLine("Candidate for reduction: " + buildStringFromMinterms(minimized[i], expr));
                    int ncontrol_frequency = 0;
                    int ncontrol = findControlVariable(term_frequency, ref ncontrol_frequency);
                    if (ncontrol == -1)
                    {
                        /* No control term found. Expression may not be reducible via Shannon expansion */
                        Console.WriteLine("No control term found, cannot reduce.");
                        Console.ReadKey();
                        return true;
                    }
                    Console.WriteLine("Control term candidate is {0},({2}) with frequency {1}.", ncontrol, ncontrol_frequency, expr.rhs.lits[ncontrol]);
                    Console.WriteLine();

                    //

                    Console.WriteLine("* Partitioning pass:");
                    var npartitions = partitionMinterms(minimized[i], expr, ncontrol);

                    /* Display unminimized partitions */
                    Console.WriteLine("Partitions:");
                    for (int ii = 0; ii < 3; ii++)
                    {
                        Console.Write("{0}: ", ii);
                        foreach (var x in npartitions[ii])
                            Console.Write("{0} ", x);
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }
            }
#endif

            return false;
        }


        
        public string expand()
        {
            string result = "";
            expand(input, ref result);
            return result;
        }
    }


} // End
