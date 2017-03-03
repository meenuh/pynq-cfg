using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// recursive factoring
// control term partitions expression, then ther'es additional or terms
// for the subexpressions associated with the control terms they can be reduced
// so each 1 eqn splits into 2, etc.

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



        public List<List<int>> findVariableFrequency(List<Minterm> minterms, int max_variables)
        {
            const int max_states = 2;

            List<List<int>> term_frequency = new List<List<int>>();

            /* Frequency list has two sub-lists for true and false case */
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
                    else if (minterm.data[i] == '1') term_frequency[1][i]++;
                }
            }

            return term_frequency;
        }


        public int findControlVariable(List<List<int>> term_frequency, ref int control_frequency)
        {
            /* Find control term.
             * This is accomplished by finding the minimum frequency for each variable,
             * and then locating the largest overall frequency across all variables.
             */
            int control = -1;
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
            return control;
        }


        public List<List<Minterm>> minimizePartitions(List<List<Minterm>> partitions, Expression expr)
        {
            int max_lits = expr.rhs.lits.Count();

            var minimized = new List<List<Minterm>>();
            for (int i = 0; i < 3; i++)
                minimized.Add(new List<Minterm>());

            /* Minimize partitions */
            for (int i = 0; i < 3; i++)
            {
                foreach (Minterm parent in partitions[i])
                {
                    foreach (Minterm child in partitions[i])
                    {
                        int diff = 0;
                        int diff_pos = 0;

                        /* Don't compare the same minterm to itself */
                        if (parent == child)
                            continue;

                        /* Count number of differences between parent and child, stop if more than one found */
                        for (int index = 0; index < max_lits && diff <= 1; index++)
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
                            derived.used = false;

                            /* If we don't already have this minterm, add it and mark parent and child as being reduced */
                            if (!minimized[i].Contains(derived))
                            {
                                parent.used = true;
                                child.used = true;
                                minimized[i].Add(derived);
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

                Console.Write("Could not minimize: ");
                foreach(Minterm remaining in partitions[i])
                {
                    Console.Write("{0} ({1}) ", remaining, remaining.used);
                }
                Console.WriteLine();

                /* Add any minterms we couldn't minimize to the output */
                foreach (Minterm remaining in partitions[i])
                {
                    if (remaining.used == false && !minimized[i].Contains(remaining))
                        minimized[i].Add(remaining);
                }
            }
            return minimized;
        }




        public List<List<Minterm>> partitionMinterms(List<Minterm> minterms, Expression expr, int control)
        {
            var partitions = new List<List<Minterm>>();

            /* Build list of three minterms for each of 0, 1, x */
            partitions.Add(new List<Minterm>());
            partitions.Add(new List<Minterm>());
            partitions.Add(new List<Minterm>());

            foreach (Minterm minterm in minterms)
            {
                /* Make new minterm with control term replaced by don't-care */
                Minterm modified = new Minterm(minterm, control, 'x');

                /* Add new minterm to appropriate partition */
                if (minterm.data[control] == '0') partitions[0].Add(modified);
                else
                if (minterm.data[control] == '1') partitions[1].Add(modified);
                else
                if (minterm.data[control] == 'x') partitions[2].Add(modified);
            }

            return partitions;
        }


        public string buildStringFromMinterms(List<Minterm> minterms, Expression expr)
        {
            string output = "";
            int max_lits = expr.rhs.lits.Count();

            foreach (Minterm minterm in minterms)
            {
                bool wrote = false;
                for (int i = 0; i < max_lits; i++)
                {
                    if (minterm.data[i] != 'x')
                    {
                        if (minterm.data[i] == '0')
                            output += '~';
                        output += expr.rhs.lits[i];
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

        public string buildMinimizedExpressionStr(List<List<Minterm>> minimized, Expression expr, int control)
        {
            string output = "";
            int max_lits = expr.rhs.lits.Count();

            /* Output parts of expression that were partitioned by the control term */
            for (int j = 0; j < 2; j++)
            {
                var minterm_list = minimized[j];   

                /* The first partition is associated with the control term being false */
                if (minterm_list == minimized[0])
                    output += "~";

                /* Output start of control term */
                output += String.Format("{0}(", expr.rhs.lits[control]);

                /* Output minterm */
                output += buildStringFromMinterms(minterm_list, expr);

                /* Output end of control term */
                output +=  ")";

                /* Insert OR term between two partitions */
                if (j == 0)
                    output +=  " | ";
            }

            /* Output remaining parts that are unaffected by the control term */
            foreach (var minterm in minimized[2])
            {
                string temp = "";
                for(int i = 0; i < max_lits; i++)
                {
                    if (minterm.data[i] == '0') temp += "~" + expr.rhs.lits[i];
                    else
                    if (minterm.data[i] == '1') temp += expr.rhs.lits[i];
                }
                if (!String.IsNullOrEmpty(temp))
                    output += " | " + temp;
            }
                
            return output;
        }



        class Mux
        {
            public Expression parent;           /* Parent expression */
            public Expression muxa;             /* Input A */
            public Expression muxb;             /* Input B */
            int control;                        /* Control term */
            public List<Expression> remainder;  /* Additional minterms OR'd together */
        };




        public bool expand(string input, ref string result)
        {
            result = null;
            Expression expr = new Expression();

            //============================================================================
            /* Parse string into expression */
            //============================================================================
            expr.parseString(input);

            //============================================================================
            /* Build minterm list from expression */
            //============================================================================
            var minterms = makeMintermsFromExpression(expr);
            
            //============================================================================
            /* Calculate frequency of variables used */
            //============================================================================
            var term_frequency = findVariableFrequency(minterms, expr.rhs.lits.Count());

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
            for (int i = 0; i < term_frequency.Count; i++)
            {
                Console.Write("Term frequency {0}: ", i);
                foreach (int freq in term_frequency[i])
                    Console.Write("{0} ", freq);
                Console.WriteLine();
            }

            //============================================================================
            /* Locate control term */
            //============================================================================
            int control_frequency = 0;
            int control = findControlVariable(term_frequency, ref control_frequency);
            if (control == -1)
            {
                /* No control term found. Expression may not be reducible via Shannon expansion */
                Console.WriteLine("No control term found, cannot reduce.");
                Console.ReadKey();
                return true;
            }
            Console.WriteLine("Control term candidate is {0} with frequency {1}.", control, control_frequency);
            Console.WriteLine();

            //============================================================================
            /* Partition minterms by control signal */
            //============================================================================
            Console.WriteLine("* Partitioning pass:");
            var partitions = partitionMinterms(minterms, expr, control);
               
            /* Display unminimized partitions */
            Console.WriteLine("Partitions:");
            for (int i = 0; i < 3; i++)
            {
                Console.Write("{0}: ", i);
                foreach (var x in partitions[i])
                    Console.Write("{0} ", x);
                Console.WriteLine();
            }
            Console.WriteLine();

            //============================================================================
            /* Minimize partitions to remove redundant minterms*/
            //============================================================================
            Console.WriteLine("* Minimization pass:");

            var minimized = minimizePartitions(partitions, expr);

            /* Display minimized partitions */
            Console.WriteLine("Minimized partitions:");
            for (int i = 0; i < 3; i++)
            {
                Console.Write("{0}: ", i);
                foreach (var x in minimized[i])
                    Console.Write("{0} ", x);
                Console.WriteLine();
            }
            Console.WriteLine();

            //----------------------------------------------------------
            Console.WriteLine("* Minimized expression:");         
            Console.WriteLine("Control term: {0}", expr.rhs.lits[control]);
            Console.Write("Expression: {0}", buildMinimizedExpressionStr(minimized, expr, control));
            Console.WriteLine();

            //--------------------------------------------------------
            //--------------------------------------------------------
            //--------------------------------------------------------
            // need to make sub expression based on
            // results, with new lits 
            // 
            // Temporary: hard-code second pass
            // Check if the minterms partitioned by the control term have more than one minterm



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
