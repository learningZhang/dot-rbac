﻿using Grit.Sequence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;

namespace Grit.Tree
{
    public class TreeService : ITreeService
    {
        public TreeService(string table,
            ISequenceRepository sequenceRepository,
            //ITreeRepository treeRepository,
            Ninject.IKernel kernel)
        {
            this.Table = table;
            this.SequenceRepository = sequenceRepository;
            //this.TreeRepository = treeRepository;
            TreeRepository = kernel.Get<ITreeRepository>(new Ninject.Parameters.ConstructorArgument("table", table));
        }

        public string Table { get; private set; }
        private ISequenceRepository SequenceRepository { get; set; }
        private ITreeRepository TreeRepository { get; set; }

        public Node GetTree(int tree)
        {
            var nodes = TreeRepository.GetTreeNodes(tree);
            Node root = nodes.FirstOrDefault();
            if(root == null)
            {
                root = new Node(tree);
            }
            foreach(var node in nodes.Skip(1))
            {
                root.AddChild(node);
            }
            return root;
        }

        public void SaveTree(Node root)
        {
            IList<Node> nodes = new List<Node>();
            root.Each(x=>nodes.Add(x));
            TreeRepository.SaveTreeNodes(nodes);
        }
    }
}
