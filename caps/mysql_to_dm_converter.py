#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
MySQL到DM数据库转换工具
主程序入口
"""

import os
import sys
import json
import argparse
import logging
from pathlib import Path
from typing import List, Dict

# 添加当前目录到Python路径
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from utils.sql_converter import SQLConverter
from utils.file_processor import FileProcessor


class MySQLToDMConverter:
    """MySQL到DM转换器主类"""
    
    def __init__(self, config_dir: str = "config"):
        self.config_dir = config_dir
        self.data_type_mapping = {}
        self.conversion_rules = {}
        self.converter = None
        self.processor = None
        
        # 加载配置
        self._load_configurations()
        
        # 初始化转换器
        self._initialize_converter()
    
    def _load_configurations(self):
        """加载配置文件"""
        try:
            # 加载数据类型映射
            data_type_file = os.path.join(self.config_dir, "data_type_mapping.json")
            with open(data_type_file, 'r', encoding='utf-8') as f:
                self.data_type_mapping = json.load(f)
            
            # 加载转换规则
            conversion_rules_file = os.path.join(self.config_dir, "conversion_rules.json")
            with open(conversion_rules_file, 'r', encoding='utf-8') as f:
                self.conversion_rules = json.load(f)
                
        except FileNotFoundError as e:
            print(f"配置文件未找到: {e}")
            sys.exit(1)
        except json.JSONDecodeError as e:
            print(f"配置文件格式错误: {e}")
            sys.exit(1)
    
    def _initialize_converter(self):
        """初始化转换器"""
        self.converter = SQLConverter(self.data_type_mapping, self.conversion_rules)
        self.processor = FileProcessor(self.converter)
    
    def convert_single_file(self, source_file: str, target_file: str) -> bool:
        """转换单个文件"""
        try:
            # 确保目标目录存在
            target_dir = os.path.dirname(target_file)
            if target_dir:
                os.makedirs(target_dir, exist_ok=True)
            
            # 读取源文件
            with open(source_file, 'r', encoding='utf-8') as f:
                content = f.read()
            
            # 转换内容
            filename = os.path.basename(source_file)
            converted_content = self.converter.convert_sql_file(content, filename)
            
            # 写入目标文件
            with open(target_file, 'w', encoding='utf-8') as f:
                f.write(converted_content)
            
            print(f"转换完成: {source_file} -> {target_file}")
            return True
            
        except Exception as e:
            print(f"转换失败: {source_file} - {str(e)}")
            return False
    
    def convert_directory(self, source_dir: str, target_dir: str) -> Dict:
        """转换整个目录"""
        return self.processor.process_directory(source_dir, target_dir)
    
    def convert_project(self, project_root: str) -> Dict:
        """转换整个项目"""
        directory_mappings = []
        
        # 查找所有mysql目录
        for root, dirs, files in os.walk(project_root):
            for dir_name in dirs:
                if dir_name == "mysql":
                    mysql_dir = os.path.join(root, dir_name)
                    dmsql_dir = os.path.join(root, "dmsql")
                    directory_mappings.append((mysql_dir, dmsql_dir))
        
        if not directory_mappings:
            print("未找到mysql目录")
            return {}
        
        print(f"找到 {len(directory_mappings)} 个mysql目录")
        
        # 处理所有目录
        return self.processor.process_multiple_directories(directory_mappings)
    
    def validate_conversion(self, source_file: str, target_file: str) -> bool:
        """验证转换结果"""
        return self.processor.validate_conversion(source_file, target_file)


def main():
    """主函数"""
    parser = argparse.ArgumentParser(description="MySQL到DM数据库转换工具")
    parser.add_argument("--source", "-s", required=True, help="源文件或目录路径")
    parser.add_argument("--target", "-t", help="目标文件或目录路径")
    parser.add_argument("--project", "-p", help="项目根目录路径")
    parser.add_argument("--validate", "-v", action="store_true", help="验证转换结果")
    parser.add_argument("--verbose", action="store_true", help="详细输出")
    
    args = parser.parse_args()
    
    # 设置日志级别
    log_level = logging.DEBUG if args.verbose else logging.INFO
    
    # 创建转换器
    converter = MySQLToDMConverter()
    
    if args.project:
        # 转换整个项目
        print(f"开始转换项目: {args.project}")
        stats = converter.convert_project(args.project)
        
        if stats:
            print("\n转换完成!")
            print(f"总文件数: {stats['total_files']}")
            print(f"成功转换: {stats['converted_files']}")
            print(f"转换失败: {stats['failed_files']}")
            print(f"成功率: {stats['converted_files']/stats['total_files']*100:.2f}%")
        
    elif args.source and args.target:
        if os.path.isfile(args.source):
            # 转换单个文件
            success = converter.convert_single_file(args.source, args.target)
            if success and args.validate:
                is_valid = converter.validate_conversion(args.source, args.target)
                print(f"验证结果: {'通过' if is_valid else '失败'}")
        else:
            # 转换目录
            stats = converter.convert_directory(args.source, args.target)
            print(f"转换完成! 成功: {stats['converted_files']}, 失败: {stats['failed_files']}")
    
    else:
        parser.print_help()


if __name__ == "__main__":
    main()
