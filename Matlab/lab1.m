town = struct(
'town', {'Минск', 'Гомель', 'Могилёв', 'Витебск', 'Гродно', 'Брест'}, 
'obl', {'', 'Гомельская', 'Могилёвская', 'Витебская', 'Гродненская', 'Брестская'}, 
'distr', {'', 'Гомельский', '', 'Витебский', '', ''}, 
'admin', {'столица', 'город областного подчинения', 'областной центр', 'областной центр', 'областной центр', 'город областного подчинения'}, 
'square', {348.84, 139.77, 118.5, 134.601, 142.11, 146.12}, 
'citizen', {1982444, 535693, 381353, 377392, 370919, 347576}, 
'rail_st', {true, true, true, true, true, true}, 
'aero_st', {true, true, true, true, true, true}, 
'water_st', {false, false, false, false, true, true}, 
'n_school', {273, 77, 45, 49, 43, 31}, 
'n_univers', {23, 7, 7, 5, 4, 2}, 
'salary', {1018.4, 892.7, 833.2, 846.7, 857.6, 853.2}, 
'dist_mnk', {0, 280.67, 181.65, 222.18, 247.88, 325.82}, 
'dist_mcw', {676.2, 568.27, 510.48, 471.99, 915.87, 994.7}, 
'dist_brl', {954.7, 1189.15, 1136.06, 1139, 706.74, 703.98}, 
'coord', {'53-27', '52-30', '53-30', '55-30', '53-23', '52-23'}
);
c = cell(2, 6);
[c{1, :}] = deal(town.dist_mcw);
[c{2, :}] = deal(town.town);
[~, index] = sort(cell2mat(c(1, :)), 'descend');
c = c(:, index);
x = [1:6];
y = cell2mat(c(1, :));
scatter(x, y);
xticklabels([]);
xticks([]);
xlim([0, 7]);
text(x + 0.1, y, c(2, :));