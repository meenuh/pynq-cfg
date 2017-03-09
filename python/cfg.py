from collections import Counter

expr = 'f=~a * ~b & ~c | a & ~b * c + a & b & ~c | a & b & c | d'
ops = {'*': '&', '\'': '~', '+': '|'}          # add operators to dictionary


class Cfg:

    expression = ""
    terms = ""

    def __cleanup_expression(self):

        self.expression = self.expression.replace('*', '&')
        self.expression = self.expression.replace('\'', '~')
        self.expression = self.expression.replace('+', '|')

        # remove operators
        self.terms = self.expression.replace('&', '')
        self.terms = self.terms.replace('~', '')
        self.terms = self.terms.replace('|', '')

        print("expression is " + self.expression)
        print("terms are: " + self.terms)
        return

    @staticmethod
    def __generate_minterms(self):

        return

    # this is the actual function that will be exposed
    def bool_func(self, bool_expr):

        # remove lhs to work with only what we care about
        temp_list = bool_expr.split("=")
        self.expression = temp_list[1]
        self.expression = "".join(self.expression.split())  # remove whitespaces

        self.__cleanup_expression()

        c = Counter()

        return

test = Cfg()
Cfg.bool_func(test, expr)
