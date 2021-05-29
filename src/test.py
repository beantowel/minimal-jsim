class dInterval:
    def __init__(self):
        self.intervals = []

    def isIntersect(self, i1, i2):
        if i2[0] >= i1[0] and i2[0] <= i1[1]:
            return True
        if i2[1] >= i1[0] and i2[1] <= i1[1]:
            return True
        return False

    def merge(self, i1, i2):
        return [min(i1[0], i2[0]), max(i1[1], i2[1])]

    def complement(self, i1, i2):
        newIntervals = []
        if i1[0] < i2[0]:
            newIntervals.append([i1[0], min(i1[1], i2[0])])
        if i1[1] > i2[1]:
            newIntervals.append([max(i1[0], i2[1]), i1[1]])
        return newIntervals

    
    def maintain(self):
        newIntervals = []
        for intv in list(sorted(self.intervals, lambda x: x[0])):
            if len(newIntervals) == 0:
                newIntervals.append(intv)
            elif self.isIntersect(intv, newIntervals[-1]):
                newIntervals[-1] = self.merge(intv, newIntervals[-1])
        self.intervals = newIntervals
        
    def add(self, frOm, to):
        self.intervals.append([frOm, to])
        self.maintain()
        return self.intervals

    def remove(self, frOm, to):
        newIntervals = []
        for intv in self.intervals:
            if self.isIntersect(intv, [frOm, to]):
                newIntervals.extend(self.complement(intv, [frOm, to]))
            else:
                newIntervals.append(intv)
        self.intervals = newIntervals
        self.maintain()
        return self.intervals

x = dInterval()
print(x.add(1, 5))
print(x.remove(2, 3))
print(x.add(6, 8))
print(x.remove(4, 7))
print(x.add(2, 7))